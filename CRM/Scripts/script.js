(function ($) {
    $.fn.nextInput = function () {
        var controls = $('input:inputControls');
        var index = controls.index(this) + 1;
        return controls.eq(index);
    };

    window.currentUser = new function() {
        this.FullName = '';
        this.UserId = 0;
        this.UserVerticalRelId = 0;
        this.Company = '';
        this.isAdmin = false;
        this.isClientUser = false;
        this.UserType = 0;
        this.UserRole = 0;
        this.VerticalId = 0;
        this.VerticalName = '';
        this.ClientParentId = 0;

        this.exportOptions = {
            ignoreColums: ["Address", "_id_"],
            ignoreQuestionIds: null
        };

        var initialize = function (object) {
            var found = false;
            var cookieRegex = /(?:(\w+)=)?(\w+)=(\w+)?/g;
            var matches, type;
            while (matches = cookieRegex.exec(document.cookie)) {
                if (matches[1] === 'UserValidation') { found = true; }
                if (found) {
                    type = typeof object[matches[2]];
                    if (type === 'boolean') {
                        object[matches[2]] = Boolean(matches[3]);
                    } else if (type === 'number') {
                        object[matches[2]] = Number(matches[3]);
                    } else if (type === 'object') {
                        //This does not apply right now
                    } else {
                        object[matches[2]] = matches[3];
                    }
                }
            }
            object.isAdmin = object.UserType === 1;
            object.isClientUser = object.UserType.in(5, 6);

                /*$(["UserId", "PayoutId", "Transfered", "Disposition", "LeadSource", "VerticalId", "FBPageId", "FBCampaignId", "FBCampaignName", "FBLeadId", "FBCreatedTime"]).each(function () {
                    object.exportOptions.ignoreColums.push(this);
                });
                object.exportOptions.ignoreQuestionIds = [8];*/
        };
        initialize(this);
    };

    window.wait = function (delay, start) {
        var now = Date.now();
        start = start || now;
        delay = delay || 5000;
        if ((now - start) >= delay) { return; }
        wait(delay, start);
    };

    window.waitForMethod = function (method, cb) {
        if (window.hasOwnProperty(method)) {
            cb();
        } else {
            setTimeout(function () {
                waitForMethod(method, cb);
            }, 0);
        }
    };

    window.warnBrowserClose = function (disable) {
        if (disable === false) {
            $('.btn-allow-redirect').addClass('hidden');
            window.onbeforeunload = null;
            window.onunload = null;
        } else if (window.onunload == null) {
            window.onbeforeunload = function (e) {
                e = e || window.Event;
                e.returnValue = "You have not tranfered the patient yet?";
                return e.returnValue;
            };
            window.onunload = function() {
                if (typeof disable === 'function') {
                    disable();
                    wait(2000);
                }
            };
        }
    };

	window.PostFailure = function (resp) {
		$.post('/ajax/getview/_ModalDialog', { error: resp.responseText }, handleResponse);
    };

    window.callAsync = function (handler, delay) {
        window.clearTimeout(window.asyncTmr);
        window.asyncTmr = window.setTimeout(handler, delay || 20);
    };

    window.successRedirect = function (data) {
        if (typeof data === 'object') {
            if (data.redirect) {
                warnBrowserClose(false);
				$('html').css({ cursor: 'wait' });
				$('#pleaseWaitDialog').modal();
                window.location.replace(data.redirect);
            }
        } else {
            handleResponse(data);
        }
    };

	window.handleResponse = function (data, status, xhr) {
		if (typeof data === 'object') {
            if (data.redirect) {
                successRedirect(data);
			} else {
				$(document).triggerHandler('onHandleResponse', [this.url, data]);
			}
        } else {
            var modal = $(data);
			if (modal.is('[append-form="true"]')) {
				$('form:first').append(modal);
			} else {
				$('body').append(modal);
			}
			
            modal.find('.typeahead').setupTypeahead();
            var form = modal.find('form').refreshValidation();
            if (form.exists()) {
                form.find('.btn-group.radio-group input:radio:checked').closest('.btn.radio-group').toggleClass('btn-success btn-primary');
                form.find('#Customer_CustomerId:input:hidden').each(function () {
                    var userIdInput = $('form:first #UserId:input:hidden');
                    if (userIdInput.exists()) {
                        /*
                            Make sure we set the UserId value of the main form
                            to prevent multiple customers from being created
                            when clicking on the first validate button
                        */
                        userIdInput.val(this.value);
                    }
                });
            }

            var postForm = function (e) {
                var $this = $(this);
                var modal = $this.closest('.modal');
                var form = $this.closest('form');
                if (!form.exists()) { form = modal.find('form'); }
                if (form.exists()) {
                    if (form.valid()) {
                        var url = $this.data('ajax-url') || form.data('ajax-url');
                        var success = $this.data('ajax-success') || form.data('ajax-success');
                        var callback = window[success];
                        modal.modal('hide');
                        form.postForm(url, function (resp) {
                            warnBrowserClose(false);
                            if (typeof callback === 'function') {
                                callback.call(this, resp);
                            }
                        });
                    }
				} else {
					modal.modal('hide');
                }
            };

            modal.on('show.bs.modal', function () {
                modal.setFocusFirstInput(1000);
			}).on('hidden.bs.modal', function () {
                var $this = $(this);
                $this.closest('form.form-customer').addClass('watch-change');
				var url = $this.data('redirect');
				$this.remove();
                if (url) { successRedirect({ redirect: url }); }
            }).on('click', '.btn.submit', function (e) {
                postForm.call(this, e);
            }).on('click', '.btn.redirect', function (e) {
                var url = $(this).data("redirect");
				if (url) { successRedirect({ redirect: url }); }
				modal.modal('hide');
            }).modal({ backdrop:false });
            return modal;
        }
    };

    window.onerror = function (err, url, line, column, stackTrace) {
		//var err = 'Unhandled JavaScript Exception: {0}'.format(msg.replaceAll(/^uncaught exception: /i, ''));
		stackTrace = "{0}:line:{1}\r\t".format(url, line, (stackTrace || ''));
		
		console.error(err);
		if (line > 0) {
			var $form = $('form');
			var fromData = '';
			if ($form.exists()) {
				fromData = JSON.stringify($form.toJSON());
			}
			var params = {
				url: window.location.href,
				form: fromData,
				error: err,
				stacktrace: stackTrace
            };
            $.post('/ajax/error', params);
            onAjaxComplete();
            return true;
        }
    };

    var onAjaxComplete = function () {
		$('form').removeClass('ignore runQuiet');
		$('html').css({ cursor: 'default' });
		$('#pleaseWaitDialog').modal('hide');
    };

    $.ajaxRequest.defaults.error = PostFailure;
    $.validator.setDefaults({ ignore: [], onfocusout:false });

	$(document).ready(function () {
		var form = $('form').bind('invalid-form.validate', function (error, element) {
			var $this = $(this);
			if ($this.hasClass('ignore')) {
				$this.removeClass('ignore');
				//$this.find('.input-validation-error').removeClass('input-validation-error');
				//$('.btn-customerIntro').click();
			} else {
				var div = $('<div><span>The following fields were found to have errors:</span><ul></ul></div>');
				div.attr('id', 'signupalert')
					.addClass('alert alert-danger alert-dismissible validation-summary-errors');
				var ul = div.find('ul');
				var qNums = [];
				$(element.errorList).each(function () {
					var group = $(this.element).closest('.question-group');
					var msg = group.data('message');
					if (!msg) {
						var question = group.find('.parentquestion').text();
						var match = /^(\d+)\)/ig.exec(question);
						if (match) {
							msg = this.message.replaceAll('{num}', match[1]);
						} else {
							msg = this.message.replaceAll('{num}', 1);
						}
					}
					if (!qNums.contains(msg)) {
						qNums.push(msg);
						ul.append('<li>{0}</li>'.format(msg));
					}
				});
				$('.ignore-validation').toggleClass('ignore-validation requiredQuestion');
				$('#validationDialog .modal-body').empty().append(div);
				$('#validationDialog').modal('show');
			}
		}).each(function () {
			this.reset();
		});

		//Make sure the form resets if user changes data and presses F5
		if (form.length) {
			window.setTimeout(function () {
				form.find('input[autocomplete="off"]').removeAttr('readonly');
			}, 500);
			form.removeClass('ignore runQuiet').get(0).reset();
		}

		var match;
		//If url is an update page and ends with an Id, disable ALL inputs and show the click to edit button
		if (match = /\/update\/(\w+)\/\d+\/?$/i.exec(window.location.href)) {
			//$(':inputControls').prop('disabled', true).addClass('disabled');
			//$('legend:first').append("<button id='btn-edit' title='Edit' class='glyphicon glyphicon-edit{0}'></button>".format(appendClass));
		} else if (/\/search\//i.test(window.location.href)) { //on a search page, click the submit button
			if (window.location.search) {
				$('form input:visible').each(function () {
					var id = this.id;
					var regEx = new RegExp('{0}=(\\w+)'.format(this.name), "i");
					var match = regEx.exec(window.location.search);
					if (match) { this.value = match[1]; }
				});
			}
			$('.panel.panel-default:visible button:submit').click();
		} else if (match = /\/create\/(\w+)\/?$/i.exec(window.location.href)) {
			if (/^customer$/i.test(match[1])) {
				$('.btn-customerIntro').click();
			}
		}

		var $fullName = $('.fullName');
		if ($fullName.length) {
			var firstName = $('#FirstName').val();
			if (firstName) {
				var lastName = $('#LastName').val();
				if (lastName) {
					var fullName = '{0} {1}'.format(firstName, lastName);
					$fullName.html(fullName);
				}
				$('.firstName').html(firstName);
			}
		}

		$('input, textarea').on('focus', function () {
			$(this).select();
		});
		$('form:visible:first').setFocusFirstInput();
	}).on('click', '.btn-allow-redirect', function (e) {
		successRedirect({ redirect: '/' });
	}).on('keypress', 'form input', function (e) {
		if (e.keyCode == 13) {
			var $this = $(this);
			if (!$this.valid()) { return false; }
			window.lastObject = $this;
			$this.nextInput().select().focus();
			return false;
		}
	}).ajaxStart(function (e) {
		if ($('form.runQuiet').exists()) { return; }
		$('html').css({ cursor: 'wait' });
		$('#pleaseWaitDialog').modal();
		if (/\/search\//i.test(window.location.href)) {
			if (/\/search\/client/i.test(window.location.href)) {
				$("#customerSearchResults:visible").hide().find('#customerResults').empty();
				$("#callacenterSearchResults:visible").hide().find('#callcenterResults').empty();
				$("#clientSearchResults").show();
			} else if (/\/search\/customer/i.test(window.location.href)) {
				$("#callacenterSearchResults:visible").hide().find('#callcenterResults').empty();
				$("#clientSearchResults:visible").hide().find('#clientResults').empty();
				$("#customerSearchResults").show();
			} else if (/\/search\/callcenteruser/i.test(window.location.href)) {
				$("#clientSearchResults:visible").hide().find('#clientResults').empty();
				$("#customerSearchResults:visible").hide().find('#customerResults').empty();
				$("#callacenterSearchResults").show();
			}
		}
		$('.validation-summary-errors').removeClass('validation-summary-errors').addClass('validation-summary-valid');
	}).ajaxStop(onAjaxComplete); //.ajaxError(onAjaxComplete);
})(jQuery);