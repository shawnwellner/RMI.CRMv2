(function ($) {
    'use strict';
    angular.module("customer", [])
    .directive('transferButton', function ($parse) {
        return {
            restrict: 'A',
            scope: true,
            link: function (scope, element, attrs) {
                scope.$watch("location", function (newValue, prevValue) {
                    if (newValue) {
                        element.find('span:last').text(newValue.Transfered ? "UnTransfer" : "Transfer");
                        if (!currentUser.UserType.in(1, 2)) { //Not Admin or Manager
                            attrs.$set('ngDisabled', true);
                        }
                    }
                });
            }
        };
    }).controller('listofproviders', function ($scope) {
        $('form').on("change", function (e) {
            if ($(this).hasClass('watch-change')) {
                var target = $(e.target);
                var isForm = target.is('form');
                if (!target.hasClass('ignore-change') && (isForm || target.hasClass('provider-lookup') ||
                     target.closest('.question-container').exists())) {
                    //$('.btn-quick-save').closest('.btn-group').remove();
                    //$('.btn-quick-save').addClass('btn-quick-save');
                    $('#Latitude').val('');
                    $('#Longitude').val('');
                    $('#providers').addClass('hidden');
                    $('#scriptText').removeClass('hidden')
                                    .find('.not-qualified').addClass('hidden').end()
                                    .find('.form-change').showIf(!isForm);

                    $('.btn-quick-save').closest('.btn-group').removeClass('hidden');
                    $scope.location = null;
                    $scope.script = null;
                    $scope.listOfProviders = null;
                    $scope.$apply();
                }
            }
        });

        var setAllowWarmTransfer = function (enabled, notQualified) {
            var q, selector;
            var nqValue = (notQualified.length > 0).toString();
            $('#NotQualified').attr('value', nqValue).val(nqValue);
            var group = $('.question-group');
            group.find('span.parentquestion').addClass('qualified');
            for (var i = 0, l = notQualified.length; i < l; i++) {
                q = notQualified[i];
                group.filter('[questionid={0}]'.format(q.QuestionId))
                        .find('input.input-qualified').val('false').end()
                        .find('span.qualified').toggleClass('qualified notQualified');
            }
            
            $('#providers').showIf(enabled);
            $('.btn-quick-save').closest('.btn-group').showIf(enabled);
            $('#scriptText').showIf(!enabled)
                            .find('.not-qualified').showIf(!enabled).end()
                            .find('.form-change ').showIf(enabled);

            /*if (enabled) {
                $('#scriptText').addClass('hidden');
                $('#providers').removeClass('hidden');
                $('.btn-quick-save').closest('.btn-group').removeClass('hidden');
            } else {
                $('.btn-quick-save').closest('.btn-group').addClass('hidden');
                $('#providers').addClass('hidden');
                $('#scriptText').removeClass('hidden')
                                .find('.not-qualified').removeClass('hidden').end()
                                .find('.form-change ').addClass('hidden');
            }*/
        };

        window.onCustFormSubmitSuccess = function (data) {
            document.getElementById('ClientVerticalRelId').value = null;
            document.getElementById('OfficeLocationId').value = null;
            document.getElementById('VCLRelId').value = null;
            document.getElementById('AllowEmailTransfer').value = null;
            document.getElementById('Distance').value = null;
            window.scrollTo(0, 0);
            if (!data) { return; }
            if (typeof data === 'object') {
                if (data.invalidZipcode) {
                    if (data.reason === 'nomatch') {
                        $('#State').addClass(data.reason);
                    }
                    $('#Zip').addClass(data.reason).closest('form').valid();
                } else if (data.redirect) {
                    onFormSubmitSuccess(data);
				} else if (data.customerLocation) {
					handleResponse(data.dialog);
                    //successRedirect({ redirect: "/error/" });
                    $('#Latitude').val(data.customerLocation.Latitude);
                    $('#Longitude').val(data.customerLocation.Longitude);
                    $('.question-container .question-group input.input-qualified').each(function () {
                        this.value = '';
                    });

                    $scope.location = null;
                    $scope.listOfProviders = data.listOfProviders.length > 1 ? data.listOfProviders : null;
                    $scope.selectProvider(data.listOfProviders[0]);
                    setAllowWarmTransfer(data.listOfProviders.length > 0, data.notQualified);
                    $scope.$apply();
                } else {
                    throw new Error('Unexpected point in code');
                }
            } else {
                handleResponse(data);
            }
        };

        $scope.selectProvider = function (location, e) {
            $scope.location = location;
            var $form = $('form.form-customer');
            if (!location) {
                $form.addClass('watch-change');
                return;
            }
            window.callAsync(function () {
                updateMap(location);
            }, 200);
            if (e) {
                var $this = $(e.target);
                $this.closest('.dropdown-menu').find('.glyphicon.glyphicon-ok').toggleClass('glyphicon-ok glyphicon-home');
                $this.find('.glyphicon').toggleClass('glyphicon-ok glyphicon-home');
            }
            if (location.Transfered && window.currentUser.isAdmin) {
                $form.removeClass('watch-change');
                $('#btn-fbgetProviderList').closest('.form-group').addClass('hidden');
            } else {
                $form.addClass('watch-change');
                $('#btn-fbgetProviderList').closest('.form-group').removeClass('hidden');
                $form.find('#ClientVerticalRelId').val(location.ClientVerticalRelId);
                $form.find('#OfficeLocationId').val(location.OfficeLocationId);
                $form.find('#VCLRelId').val(location.VCLRelId);
                $form.find('#Distance').val(location.Distance);
            }
        };

        $scope.updateCustomer = function (location, e) {
            var $this = $(e.currentTarget);
            var dispositionId = $this.data('dispositionid');
            var additionalData = null;
            var $form = $this.closest('form');
            if ($form.exists()) {
                if ($this.hasClass('not-qualified')) {
                    $form.find('#NotQualified').val("true");
                } else if (location) {
                    $form.find('#NotQualified').val("");
                    $form.find('#ClientVerticalRelId').val(location.ClientVerticalRelId);
                    $form.find('#OfficeLocationId').val(location.OfficeLocationId);
                    $form.find('#VCLRelId').val(location.VCLRelId);
                    $form.find('#AllowEmailTransfer').val(location.Provider.AllowEmailTransfer);
                    $form.find('#Distance').val(location.Distance);
					if (location.Transfered) {
						$form.removeClass('watch-change');
						$.post('/ajax/selectdisposition', null, handleResponse);
						return;
					} else if (location.Provider.ConciergeModel) {
						additionalData = { DispositionId: 39, Transfered: true }; //Transfer
                    } else if (dispositionId) {
                        additionalData = { DispositionId: dispositionId };
                    }
                }
                $form.postForm('customerupdate', handleResponse, additionalData);
            }
        };
    }).controller('search', function ($scope) {
        window.rmi.websocket.events.onTransferedChange = function (clientTransferModel, transfered) {
            if ($scope.ListofCustomers.length) {
                if (transfered) { //Add Customer to List
                    if (!$scope.ListofCustomers.find(function (cust, index) {
                        return cust.CustomerId === clientTransferModel.Customer.CustomerId;
                    })) {
                        $scope.ListofCustomers.splice(0, 0, clientTransferModel.Customer);
                    }
                } else { //Remove Customer from List
                    for (var i = $scope.ListofCustomers.length - 1; i >= 0; i--) {
                        if ($scope.ListofCustomers[i].UserId === clientTransferModel.Customer.CustomerId) {
                            $scope.ListofCustomers.splice(i, 1);
                            i++;
                        }
                    }
                }
            } else if (transfered) {
                $scope.ListofCustomers = [clientTransferModel.Customer];
            }
            $scope.$apply();
        };

        window.getCustomerTable = function (data) {
            if (typeof data === 'object' && !data.redirect) {
                $scope.ListofCustomers = data.ListofCustomers.length > 0 ? data.ListofCustomers : null;
                $('[data-provide="pagination"]').pagination(data.PageInfo);
                $scope.$apply();
            } else {
                handleResponse(data);
            }
        };

        $scope.getVerticalLetter = function (user) {
            //return user.VerticalName.toLowerCase();
            return user.VerticalName.charAt(0);
        };

        $scope.getCustomerDetails = function (user) {
            //$.getJSON("/ajax/getcustomerdata/" + user.UserId, function (results) {
            $.post('/ajax/getcustomerdetails', { userId: user.CustomerId }, function (results) {
                /*
                var form = handleResponse(results).find('form');
                form.find('.toggle-required .form-control:visible').each(function () {
                    $(this).rules('add', "required");
                });
                form.refreshValidation();
                */
                var dialog = handleResponse(results);
                if (dialog) {
                    user.Polyline = dialog.find('.googleMap').setGoogleMapContainer().data('polyline');
                    if (user.Polyline) { window.updateMap(user, true); }
                }
            });
        };
        /*
        if (window.location.search) {
            var match = /userId=(\w+)/i.exec(window.location.search);
            if (match) { $scope.getCustomerDetails({ UserId: match[1] }); }
        }*/
    });

	$.fn.refreshControls = function () {
		if (!this.is('form')) { return; }
		var form = this;
        this.find('div.input-group.date').each(function () {
            var today = new Date();
            var defaultOptions = {
                format: 'mm/dd/yyyy',
                //viewMode: 'months',
                //minViewMode: 'months',
                autoclose: true,
                startDate: new Date(1900, 0, 1),
                endDate: today,
                enableOnReadonly: false
            };

            var dp = $(this);
            var input = dp.find('input.form-control');
            var attr = input.attr('options');
            input.removeAttr('options');

            var options = attr ? $.extend(defaultOptions, JSON.parse(attr)) : defaultOptions;
            dp.datepicker(options).on('changeDate', function (dp) {
                var $input = $(this).find('input[questionid=1]');
                if ($input.exists()) {
                    var today = (new Date()).clearTime();
                    var age = Math.floor(today.subtract(dp.date).totalYears);
                    var span = $input.closest('.question-group').find('span.requiredQuestion');
                    var warningLabel = span.find('label.ageWarning');
                    if (age >= 65) {
                        span.removeClass('qualified');
                        if (!warningLabel.exists()) {
                            span.append($('<label />', { 'class': 'ageWarning', text: 'Possible Medicare/Medicaid!' }));
                        }
                    } else {
                        warningLabel.remove();
                    }
                }
            });

            var mask = options.format.replace(/\w/g, '9');
            input.mask(mask);
        });

        form.find('input.timepicker-default').datetimepicker({
            format: false,
            pickDate: false,
            useminutes: false,
            pickTime: true,
            minuteStepping: 60
        }).find('input.form-control').bind('keypress', function (e) {
            return e.kill();
        });

        /*Check all the answered questions*/
        form.find('.btn-group.radio-group input:radio:checked').closest('.btn').toggleClass('btn-success btn-primary');

        var isValid = form.addClass('ignore').validate().checkForm();
        if (isValid) {
            form.find('#btn-fbgetProviderList').click();
        } else {
            document.getElementById('ClientVerticalRelId').value = null;
            document.getElementById('OfficeLocationId').value = null;
            document.getElementById('VCLRelId').value = null;
            document.getElementById('Distance').value = null;
		}

		form.find('.question-group [parentid]:inputControls').each(function () {
            var $this = $(this);
            var parentId = $this.attr('parentid');
			if (parentId) {
				var parent = form.find('.question-group [clientInputTypeRelId="{0}"]'.format(parentId));
				if (parent.is(":radio")) {
					if (!parent.is(":checked")) {
						var dp = $this.closest('.input-group.date');
						if (dp.exists()) {
							dp.datepicker('disable', true);
						} else {
							$this.disabled(true);
						}
					}
				} else if ( !Boolean(parent.val())) {
					$this.disabled(true);
				}
            }
        });
    };

	var updateVertical = function (verticalId, container) {
        var userId = $('#UserId').val();
		$('#VerticalId').val(verticalId);
        $('form.form-customer').addClass('watch-change').trigger('change');
        var dispList = window.typeAheadLists.DispositionList;
        window.typeAheadLists.Dispositions = [];
        for (var i = 0, l = dispList.length; i < l; i++) {
            if (dispList[i].VerticalIdFlags & verticalId) {
                window.typeAheadLists.Dispositions.push(dispList[i].Disposition);
            }
		}
        $.post('/ajax/getverticalquestions', { verticalId: verticalId, customerId: userId }, function (resp) {
            container = container || $('.question-container');
            container.html(resp);
            container.closest('form')
                     .refreshValidation()
                     .refreshControls();
            $('.typeahead').setupTypeahead();
        });
    };

    var preventLeavingPage = function (e) {
        var $this = $(this);
        if ($this.val()) {
            var $form = $this.closest('form.form-customer');
            var questions = $form.find('.requiredQuestion').toggleClass('requiredQuestion ignore-validation');
            if ($form.validate().valid()) {
                warnBrowserClose(function () {
                    $form.postForm('CustomerUpdate', null, { DispositionId: 10 });
                });
                $this.unbind('change', preventLeavingPage);
                $('.btn-allow-redirect').removeClass('hidden');
            }
            questions.toggleClass('requiredQuestion ignore-validation');
        }
	};

	$(document).ready(function () {
		var $form = $('form.form-customer');
		if ($form.exists()) {
			if ($form.find('#UserId').val() === '') {
				$form.find('fieldset:first input.required').bind('change', preventLeavingPage);
			}
			$.post('/ajax/gettypeaheadlists', null, handleResponse);
		}
	}).on('click', '.nav-tabs > li', function (e) {
		var name = $(this).find('a').text().toLowerCase().replaceAll(' ', '-');
		var container = $('.question-container').empty();
		var prevName = container.data('prevName') || '';
		container.data('prevName', name);
		container.closest('fieldset').removeClass(prevName).addClass(name);

		var verticalId = $(this).attr('verticalId');
		updateVertical(verticalId, container);
	}).on('click', '.btn-quick-save', function (e) {
		var $this = $(this);
		var $form = $this.closest('form');
		if ($this.hasClass('btn-update')) {
			if ($form.valid()) {
				var scope = angular.element('#listofproviders').scope();
				if (scope && scope.location && scope.location.VCLRelId) {
					var additionalData = {
						PartialUpdate: true,
						VCLRelId: scope.location.VCLRelId,
						//ClientVerticalRelId: scope.location.ClientVerticalRelId
					}
					$form.postForm('CustomerUpdate', handleResponse, additionalData);
				}
			}
		} else {
			$('.requiredQuestion').toggleClass('requiredQuestion ignore-validation');
			if ($form.valid()) {
				$form.removeClass('watch-change');
				$.post('/ajax/selectdisposition', null, handleResponse);
			}
		}
	}).on('change', '.optionsContainer:has([parentId]) .form-control:visible:not([parentId])', function (e) { //Enable Child Inputs This should be wrapped in with the question validation functionality
		$(this).closest('.optionsContainer').find('[parentid]').disabled(!Boolean(this.value));
	}).on('change', '.radio-group > .btn', function (e) { //Enable Child Inputs This should be wrapped in with the question validation functionality
		var btn = $(this);
		if (btn.hasClass('btn-success')) { return; }
		var group = btn.closest('.radio-group');
		group.find('.btn-success').toggleClass('btn-success btn-primary');
		var radio = btn.toggleClass('btn-primary btn-success').find('input:radio')
		var clientInputTypeRelId = radio.attr('clientInputTypeRelId');
		var child = group.closest('.optionsContainer').find('[parentId="{0}"]'.format(clientInputTypeRelId));
		if (child.exists()) {
			var parentId = child.attr('parentId');
			var option = radio.attr("optionvalue");
			if (option.length) {
				var dp = child.closest('.input-group.date');
				if (option == radio.val()) {
					if (dp.exists()) {
						dp.datepicker('disable', false);
						dp.datepicker('show');
					} else {
						child.prop('disabled', false);
						radio.filter('.requiredQuestion').valid();
					}
				} else {
					if (dp.exists()) {
						dp.datepicker('disable', true);
						dp.datepicker('setDate', '');
					} else {
						child.prop('disabled', true).prop('checked', false)
					}
					if (radio.hasClass('requiredQuestion')) {
						radio.valid();
					}
				}
			}
		} else if (radio.hasClass('requiredQuestion')) {
			radio.valid();
		}
	}).on('change', 'input.check-duplicate', function () {
		var $this = $(this);
		if ($this.valid()) {
			var items = $('input.check-duplicate');
			var userId = $('#UserId').val();
			var params = { UserId: userId }, item;
			for (var i = 0, l = items.length; i < l; i++) {
				item = items[i];
				if (item.value.length === 0) { return; }
				params[item.id] = item.value;
			}

			$this.closest('form').addClass('runQuiet');

			//items.removeClass('check-duplicate'); //Remove the class to stop this event from occuring again
			$.ajaxRequest('CheckForDuplicate', params, handleResponse);
		}
	}).on('click', 'button:submit', function () {
		$('.question-group').removeClass('input-validation-error');
	}).on('click', '.modal-transfer .transfer-script .btn-group button.warm-transfer', function () {
		var $this = $(this);
		var value = $this.val().toBool();
		var sendEmail = !value && $this.val() === "email";
		var dialog = $this.closest('.modal.modal-transfer');
		var form = dialog.find('.form-transfer');
		var disp = form.find('#Customer_Disposition');
		var coordName = form.find('#Customer_PatientCoordName');
		var transfered = form.find('#Customer_Transfered').val(false);
		var sendTransfer = form.find('#EmailTransfer').val(false);

		form.find('.warm-transfer-yes, .warm-transfer-no').addClass('hidden');
		$this.closest('.btn-group').find('.btn.btn-success').toggleClass('btn-success btn-primary');

		if (value || sendEmail) {
			if (sendEmail) {
				coordName.required(false);
				disp.required(false).val('');
				if (form.valid()) {
					transfered.val(true);
					sendTransfer.val(true);
					$this.toggleClass('warm-transfer submit').trigger('click');
				}
			} else {
				coordName.required();
				disp.required().val('Talking to Client');
				warnBrowserClose(function () {
					dialog.find('button.cancel:first').trigger('click');
				});
				if (form.valid()) {
					transfered.val(true);
					form.postForm('transferCustomer', function () {
						$this.toggleClass('btn-primary btn-success');
						form.find('.warm-transfer-yes').removeClass('hidden');
						dialog.find('.modal-body').scollBottom();
						dialog.find('.panel-footer .tranfer-buttons')
							.find('.btn').each(function () {
								var btn = $(this);
								if (this.value.toBool()) {
									//btn.toggleClass('transfer submit')
									btn.find('i').html('Successful');
								} else {
									btn.find('i').html('Un-Successful');
									btn.parent().removeClass('hidden');
								}
							}).end().removeClass('hidden');
						disp.required(false).val('');
					}, true);
				}
			}
		} else {
			coordName.required(false);
			disp.required(false).val('');
			form.find('.warm-transfer-no').removeClass('hidden');
			dialog.find('.modal-body').scollBottom();
			dialog.find('.panel-footer .tranfer-buttons .btn').each(function () {
				var $this = $(this);
				if (this.value == 'true') {
					$this.find('i').html('Save');
				} else {
					$this.parent().addClass('hidden');
				}
			});
			dialog.find('.panel-footer .tranfer-buttons').removeClass('hidden');
			$this.toggleClass('btn-primary btn-success');
			form.postForm('transferCustomer', function () {
				disp.required().focus();
				//dialog.find('.panel-footer button.btn-success.transfer').toggleClass('transfer submit');
			}, true);
		}
	}).on('click', '.modal-transfer .panel-footer .btn-group button.transfer', function () {
		var $this = $(this);
		var value = $this.val().toBool();
		if (value) {
            /*$this.closest('.modal.modal-transfer')
                 .find('.form-transfer #Customer_Disposition')
                 .required(false).val('');*/
			$this.toggleClass('transfer submit').trigger('click');
		} else {
			$this.closest('.modal.modal-transfer')
				.find('.form-transfer .transfer-script .btn.no')
				.trigger('click');
		}
	}).on('click', '.modal-transfer button.cancel', function () {
		var $this = $(this);
		var form = $this.closest('.modal.modal-transfer').find('.form-transfer');

		form.find('#Customer_Disposition').required(false).val('');
		form.find('#Customer_PatientCoordName').required(false); //.addClass('required').rules('remove', 'required');
		form.find('input[name="Customer.Transfered"]').val(false);
		form.postForm('transferCustomer', true);
	}).on('change', '.customer-search .form-group :inputControls', function () {
		var $this = $(this);
		if ($this.attr('id') == 'UserId') {
			$('.customer-search .form-group .form-control:inputControls:not(#UserId)').clear();
		} else {
			$('.customer-search .form-group .form-control#UserId').clear();
		}
	}).on('change', '.validzipcode', function () {
		var $this = $(this);
		var zip = $this.val();
		if (zip) {
			var $form = $this.closest('form').addClass('runQuiet');
			$form.find('#City').val('');
			$form.find('#State').val('');
			$.post('/ajax/validatezipcode', { zipcode: zip }, handleResponse);
		}
	}).on('onHandleResponse', function (e, url, data) {
		if (/GetTypeAheadLists/i.test(url)) {
			window.typeAheadLists = data;
			var tab = $('.nav-tabs > li.active a');
			if (tab.exists()) {
				tab.trigger('click');
			} else {
				var verticalId = $('#VerticalId').val();
				updateVertical(verticalId);
			}
		} else if (/validatezipcode/i.test(url)) {
			var $form = $('form.form-customer');
			if (data.invalidZipcode) {
				if (data.reason === 'nomatch') {
					$form.find('#State').addClass(data.reason);
				}
				$form.find('#Zip').addClass(data.reason).valid();
			} else {
				$form.find('#Latitude').val(data.Latitude);
				$form.find('#Longitude').val(data.Longitude);
				$form.find('#City').val(data.City).valid();
				$form.find('#State').val(data.State).trigger('change').valid();
			}
		} else {
			console.error(url, data);

			/*var list = $('#listofproviders');
			list.find('.toggle-hidden').toggleClass('hidden');
			var ids = list.data("officeLocationIds") || [];
			ids.push(results.officeLocationId);
			list.data("officeLocationIds", ids);*/
		}
	});

    window.addSerializer = function (data, healthInsurance) {
        if (!data) { return; }
        //Custom Serializer for the ListOfQuestions
        $.extend(data, {
            serialize: function (row, headerRow) {
                var val;
                if (headerRow) {
                    $(this).each(function () {
                        val = this.Question;
                        if (!row.contains(val)) { row.push(val); }
                    });
                } else {
                    var obj = {};
                    $(this).each(function () {
                        row.push('"{0}"'.format(this.Answer));
                    });
                }
            }
        });
    };
})(jQuery);
