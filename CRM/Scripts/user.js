(function ($) {
    //-- On form submission check that post was successful and show non-Error service object messages ----
    window.onFormSubmitSuccess = function (data) {
        if (!data) { return; }
        if (typeof data === 'object') {
            if (data.redirect) {
                successRedirect(data);
            } else {
                throw "Unexpected Object Type Here";
            }
        } else {
            handleResponse(data);
        }
    };

    $(document).on('click', '.radio-group label', function () {
        $(this).find('input:radio:not(".disabled")').prop('checked', true).trigger('change');
    }).on('click', '.btn.togglePassword', function (e) {
        var parent = $(this).closest('.form-group');
        var userName = parent.prev().find("[id$='UserName']").attr('readonly', true);
        parent.find("[id$='Password']").toggleAttr('type', 'text', 'password');
        callAsync(function () {
            userName.removeAttr('readonly');
        }, 200);
        return e.kill();
    }).on('click, focus', 'input:text, textarea', function () {
        var self = $(this);
        callAsync(function () { self.select(); }, 100);
    }).on('change', "[id$='UserName'],[id$='Password']", function (e) {
        if (/ConfirmPassword/.test(this.id)) { return; }
        var $this = $(this);
        var parent = $this.closest('.form-group');
        var params = { userName: null, password: null };
        var userElem, passElem;

        if (/UserName$/.test(this.id)) {
            userElem = $this;
            passElem = parent.next().find("[id$='Password']");
        } else {
            passElem = $this;
            userElem = parent.prev().find("[id$='UserName']");
        }

        params.userName = userElem.val();
        params.password = passElem.val();

        if (params.userName && params.password) {
            $this.closest('form').addClass('runQuiet');
            $this.removeClass('input-validation-error');
            parent = userElem.closest('.form-group');
            parent.find('.duplicate-credentials').remove();
            $.post('/ajax/CheckCredentials', params, function (results) {
                if (results.Exists === true) {
                    $('<div class="duplicate-credentials"><span>User Credentials already being used</span></div>').appendTo(parent);
                    passElem.val('');
                    userElem.addClass('input-validation-error').focus();
                }
            });
        } else {
            parent.find('.duplicate-credentials').remove();
            userElem.removeClass('input-validation-error');
        }
    }).on('click', '.btn.exportcsv', function () {
        var $button = $('.btn.exportcsv:visible:first');
        var data = $button.data('export');
        if (data) {
            data.exportToCSV(window.currentUser.exportOptions.ignoreColums);
        } else {
            var $this = $(this);
            var $form = $('.panel.panel-default:visible:first form');
            var url = $form.attr('data-ajax-url') || undefined;
            var method = $form.attr('data-ajax-method') || undefined;
            var userId = $this.attr('userId');
            var property = $button.attr('property');
            var params = { includeQuestions: true, exporting: true };
            var pager = $('[data-provide="pagination"]');
            var pageInfo = pager.exists() ? pager.pagination('PageInfo') : null;

            var showAll = !$this.closest('.export-controls').find('input:checkbox').prop('checked');
            $($form.serializeArray()).each(function () {
                if (pageInfo && /^pagenum$/ig.test(this.name)) {
                    params[this.name] = showAll ? 1 : pageInfo.PageNum;
                } else if (pageInfo && /^maxrows$/ig.test(this.name)) {
                    params[this.name] = userId ? 1 : (showAll ? pageInfo.TotalRecords : pageInfo.MaxRows);
                } else {
                    params[this.name] = this.value;
                }
            });
            params.UserId = userId;
            var options = $.extend($.ajaxRequest.defaults, {
                type: method,
                url: url,
                data: JSON.stringify(params),
                success: function (results) {
                    if (results) {
                        var data = property ? (results[property] || results) : results;
                        if (!(data instanceof Array)) { data = [data]; }
                        if (typeof window.addSerializer === 'function') {
                            $(data).each(function () {
                                window.addSerializer(this.ListOfQuestions, this.HealthInsurance);
                            });
                        }
                        data.exportToCSV(window.currentUser.exportOptions.ignoreColums);
                    }
                }
            });

            $.ajax(options);
        }
    });
})(jQuery);