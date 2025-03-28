/// <reference path="jquery.unobtrusive-ajax.js" />

Boolean.prototype.iif = function (ifTrue, ifFalse) {
    return this.valueOf() ? ifTrue : ifFalse;
}

/*String Extension Methods*/
if (!String.prototype.replaceAll) {
    String.prototype.replaceAll = function (searchValue, newValue) {
        var regEx;
        if (searchValue instanceof RegExp) {
            regEx = searchValue;
        } else {
            regEx = new RegExp();
            regEx.compile(searchValue, 'gm');
        }
        return this.replace(regEx, newValue);
    };
}

if (!String.prototype.toBool) {
    String.prototype.toBool = function () {
        return /(true|yes|1|on)/i.test(this);
    };
}

if (!String.prototype.format) {
    String.prototype.format = function () {
        var str = this.toString();
        if(arguments.length == 1 && (arguments[0] instanceof Array)) {
            return String.prototype.format.apply(str, arguments[0]);
        } else {
            var regEx = new RegExp();
            for (var i = 0, l = arguments.length; i < l; i++) {
                regEx.compile("\\{" + i + "\\}", 'gm');
                str = str.replace(regEx, arguments[i]);
            }
            return str;
        }
    };
}

if(!String.prototype.formatCamelCase) {
    String.prototype.formatCmlCase = function () {
        var str = this.toString();
        return str.replace(/([a-z])([A-Z])/g, function ($1, $2, $3) {
            return '{0} {1}'.format($2, $3);
        });
    };
}

if (!String.prototype.formatPhoneNumber) {
    String.prototype.formatPhoneNumber = function () {
        var str = this.toString();
        return str.replace(/^(\d{3})(\d{3})(\d{4})$/ig, function ($1, $2, $3, $4) {
            return "({0}) {1}-{2}".format($2, $3, $4);
        });
    };
}

if (!String.prototype.toYesNo) {
    String.prototype.toYesNo = function () {
        var str = this.toString();
        return str.replace(/^(true)$/i, 'Yes').replace(/^(false)$/i, 'No');
    };
}

if (!String.prototype.toInt) {
    String.prototype.toInt = function () {
        var str = this.toString();
        return str.replace(/^(true)$/i, '1').replace(/^(false)$/i, '0');
    };
}


/*Number Extension Methods*/
if (!Number.prototype.pad) {
    Number.prototype.pad = function (count) {
        var str = '' + this;
        while (str.length < count) { str = '0' + str; }
        return str;
    };
}

if (!Number.prototype.in) {
    Number.prototype.in = function () {
        var value = this.valueOf();
        for (var i = 0, l = arguments.length; i < l; i++) {
            if (value === arguments[i]) { return true; }
        }
        return false;
    };
}

/*Date Extension Methods*/
if (!Date.prototype.format) {
    Date.prototype.format = function (f) {
        if (!this.valueOf()) { return ' '; }
        var month = new Array('January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December');
        var weekday = new Array('Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday');

        var d = this;
        return f.replace(/(yyyy|yy|mmmm|mmm|mm|m|dddd|ddd|dd|d|hh|h|ss|tt)/gi,
			function ($1) {
			    switch ($1) {
			        case 'yyyy': return d.getFullYear();
			        case 'yy': return d.getFullYear().toString().substr(2);
			        case 'MMMM': return month[d.getMonth()];
			        case 'MMM': return month[d.getMonth()].substr(0, 3);
			        case 'MM': return (d.getMonth() + 1).pad(2);
			        case 'M': return (d.getMonth() + 1);
			        case 'dddd': return weekday[d.getDay()];
			        case 'ddd': return weekday[d.getDay()].substr(0, 3);
			        case 'dd': return d.getDate().pad(2);
			        case 'd': return d.getDate();
			        case 'HH': return d.getHours().pad(2);
			        case 'H': return d.getHours();
			        case 'hh': return ((h = d.getHours() % 12) ? h : 12).pad(2);
			        case 'h': return ((h = d.getHours() % 12) ? h : 12);
			        case 'mm': return d.getMinutes().pad(2);
			        case 'ss': return d.getSeconds().pad(2);
			        case 'tt': return d.getHours() < 12 ? 'am' : 'pm';
			        case 'TT': return d.getHours() < 12 ? 'AM' : 'PM';
			        default: return ' ';
			    }
			}
		);
    };
}

if (!Date.prototype.addMonths) {
    Date.prototype.addMonths = function (m) {
        var month = this.getMonth() + m;
        this.setMonth(month);
        return this;
    };
}

if (!Date.prototype.addDays) {
    Date.prototype.addDays = function (d) {
        var day = this.getDate() + d;
        this.setDate(d);
        return this;
    };
}

if (!Date.prototype.addHours) {
    Date.prototype.addHours = function (h) {
        var mins = this.getMinutes() + (h * 60);
        this.setMinutes(mins);
        return this;
    };
}

if (!Date.prototype.addSeconds) {
    Date.prototype.addSeconds = function (s) {
        this.setSeconds(this.getSeconds() + s);
        return this;
    };
}

if (!Date.prototype.getTicks) {
    Date.prototype.getTicks = function () {
        return ticks = ((this.getTime() * 10000) + 621355968000000000);
    };
}

if (!Date.prototype.subtract) {
    Date.prototype.subtract = function (d) {
        return new TimeSpan(this.getTicks() - d.getTicks());
    };
}

if (!Date.prototype.clearTime) {
    Date.prototype.clearTime = function () {
        this.setHours(0, 0, 0, 0);
        return this;
    };
}

window.TimeSpan = function (ticks) {
    var TicksPer = {
        Millisecond: 10000,
        Second: 10000000,
        Minute: 600000000,
        Hour: 36000000000,
        Day: 864000000000
    };

    return {
        totalDays: ticks / TicksPer.Day,
        totalHours: ticks / TicksPer.Hour,
        totalMinutes: ticks / TicksPer.Minute,
        totalSeconds: ticks / TicksPer.Second,
        totalMilliseconds: ticks / TicksPer.Millisecond,
        totalYears: (ticks / TicksPer.Day) / 365,
        ticksPer: TicksPer
    };
};

if (!Array.prototype.contains) {
    Array.prototype.contains = function (value) {
        for (var i = 0, l = this.length; i < l; i++) {
            if (this[i] == value) {
                return true;
            }
        }
        return false;
    };
}

if (!Array.prototype.exportToCSV) {
    Array.prototype.exportToCSV = function (excludeColumns) {
        if (!this || this.length === 0) { return; }
        if (typeof excludeColumns === 'string') { excludeColumns = [excludeColumns]; }
        var row = [];
        var csv = [];
        var obj = this[0];
        var val;
        //Add Headers
        for (p in obj) {
            if (excludeColumns && excludeColumns.contains(p)) { continue; }
            val = obj[p];
            if (val && val.serialize) {
                val.serialize(row, true);
            } else {
                row.push('"{0}"'.format(p));
            }
        }
        csv.push(row.toString());
        for (var i = 0, l = this.length; i < l; i++) {
            row = [];
            obj = this[i];
            for (p in obj) {
                if (excludeColumns && excludeColumns.contains(p)) { continue; }
                val = obj[p];
                if (typeof val === 'string') {
                    row.push('"{0}"'.format(val.trim()));
                } else if (val instanceof Date) {
                    row.push('"{0}"'.format(val.format('M/d/yyyy h:mm:ss TT')));
                } else if (val && typeof val === 'object') {
                    if (val.serialize) { val.serialize(row); }
                } else if (val != undefined && val != null) {
                    row.push('{0}'.format(val));
                } else {
                    row.push('{0}'.format(''));
                }
            }
            csv.push(row.toString());
        }
        
        var url = 'data:text/csv;charset=utf-8,' + escape(csv.join('\r'));
        var a = document.createElement('a');
        a.id = 'downloadCSV';
        a.href = url;
        a.download = 'ExportData_{0}.csv'.format((new Date()).format('MMddyyyy'));
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    };
}

(function ($) {
	/*$.fn.show = function () {
		if (this.css('display') == 'none') {
			this.css('display', 'block');
		}
		if (!this.is('.modal')) {
			this.removeClass('hidden');
		}
		return this;
	};

	$.fn.hide = function () {
		if (this.is('.modal')) {
			return this.removeClass('in');
		}
		return this.addClass('hidden');
	};*/

    $.Event.prototype.kill = function() {
        this.preventDefault();
        this.stopPropagation();
        this.stopImmediatePropagation();
        return false;
    }

    $.ajaxSetup({
        converters: {
            "text json": function (textValue) {
                if (textValue) {
                    var newValue = textValue.replace(/"\\\/Date\((-?\d+(-\d+)?)\)\\\/"/g, 'new Date($1)');
                    try {
                        return eval('(' + newValue + ')');
                    } catch (ex) {
                        return newValue;
                    }
                }
            }
        }
    });

    $.expr[':'].inputControls = function (element) {
        var $this = $(element);
        if (!$this.closest('form').exists()) { return false; }
        //var dialog = $this.closest('.ui-dialog, .modal-dialog');
        //if (dialog.exists() && !dialog.find('form').exists()) { return false; }
        if ($this.closest('.ignore-inputcontrols').exists()) { return false; }
        return $this.filter(':visible').is('input, textarea, select, button') ||
               $this.closest('[data-toggle="buttons"] > .btn').exists() ||
               $this.filter('input:checkbox').closest('.checkbox:visible').exists();
    };

    $.expr[':'].hasValue = function (element) {
        var $this = $(element);
        if ($this.is(':checkbox,:radio')) {
            return $this.is(':checked');
        } else {
            return Boolean(element.value);
        }
    };
    $.expr[':'].isEmpty = function (element) {
        var $this = $(element);
        if ($this.is(':checkbox,:radio')) {
            return !$this.is(':checked');
        } else {
            return !Boolean(element.value);
        }
    };

    $.fn.exists = function () { return this.length > 0; }
    
    $.fn.hasScrollBar = function () { return this.get(0).scrollHeight > this.outerHeight(); };

    /* Start Typeahead Overrides */
    var typeaheadEvents = {
        keydown: $.fn.typeahead.Constructor.prototype.keydown,
        input: $.fn.typeahead.Constructor.prototype.input,
        mousedown: $.fn.typeahead.Constructor.prototype.mousedown
    };

    var tmrId;
    var clearKeyPress = function () {
        this.value = '';
        this.$element.text('').data('active', null);
        this.$menu.find('.active').removeClass('active');
        this.lookup('');
    };

    $.fn.typeahead.Constructor.prototype.mouseenter = $.noop;
    $.fn.typeahead.Constructor.prototype.mousedown = function (e) {
        this.$menu.find('.active').removeClass('active');
        $(e.target).closest('li').addClass('active');
        typeaheadEvents.mousedown.call(this, e);
    };
    
    $.fn.typeahead.Constructor.prototype.input = function (e) {
        var self = this;
        var $this = this.$element;
        var value = $this.val().trim();
        if (!value) {
            clearTimeout(tmrId);
            tmrId = setTimeout(function () {
                clearKeyPress.call(self);
            }, 50);
        } else {
            typeaheadEvents.input.call(this, e);
        }
    };

    $.fn.typeahead.Constructor.prototype.keydown = function (e) {
        typeaheadEvents.keydown.call(this, e);
        switch (e.keyCode) {
            case 38:
            case 40:
                var $this = this.$element;
                //var menu = $this.closest('.typeahead').find('ul.typeahead.dropdown-menu');
                var menu = this.$menu;
                if (menu.hasScrollBar()) {
                    var target = menu.find('li.active').get(0);
                    target.parentNode.scrollTop = target.offsetTop;
                }
                break;
        }
    };
    /* END Typeahead Overrides */

    $.fn.setupTypeahead = function () {
        //Documentation: https://github.com/bassjobsen/Bootstrap-3-Typeahead
        this.each(function () {
            var $input = $(this).find('input:text');
            var source = window.typeAheadLists[$input.attr('source')];
            $input.typeahead({
                minLength: 0,
                source: source,
                items: 'all',
                //addItem: Boolean($input.attr('canadd')) ? "New Insurance Type" : false
            }).click(function (e) {
                $(this).typeahead('lookup');
            }).change(function (e) {
                var $this = $(this);
                var value = $this.val().trim();
                if (value.length) {
                    var source = $this.data('typeahead').source;
                    if (!Boolean($this.attr('canadd')) && !source.contains(value)) {
                        value = '';
                    }
                }
                if (value.length === 0) {
                    $this.data('active', '').val('').text('');
				}
            })
        });
    };

    var validateQuestion = function ($elm, group, inputs) {
        var isValid = false;
		var regex;
		var isRadio = $elm.is('input:radio');
		var isCheckbox = !isRadio && $elm.is('input:checkbox');
		var isSelect = !isCheckbox && $elm.is('select');

		var value = $elm.val();
		if (isRadio) {
            var name = $elm.attr('name');
            $elm = inputs.filter('input:radio[name="{0}"]:checked'.format(name));
			isValid = $elm.exists();
		} else if (isCheckbox) {
            $elm = inputs.closest('.checkbox-group').find(':checked');
            isValid = $elm.exists();
        } else if (isSelect) {
            value = $elm.find(':checked').val();
            isValid = Boolean(value);
        } else {
            var datePicker = group.find('div.input-group.date');
            if (datePicker.exists()) {
                //var format = $('div.input-group.date').data('datepicker').o.format;
                var format = datePicker.data('datepicker').o.format;
                regex = new RegExp(format.replace(/\w/g, '\\d'));
                isValid = regex.test(value);
                if (isValid) { datePicker.datepicker('hide'); }
            } else if (true === (isValid = Boolean(value))) {
                var pattern = $elm.attr('data-val-regex-pattern');
                if (pattern) {
                    var regex = new RegExp(pattern);
                    isValid = regex.test(value);
                }
            }
		}

        if (isValid) {
			var clientInputTypeRelId = $elm.attr('clientInputTypeRelId');
			var validate = isRadio ? $elm.attr('optionvalue') == $elm.val() : true;
			var children = group.find('[parentId="{0}"]'.format(clientInputTypeRelId));
			var child;
			for (var i = 0, l = children.length; i < l; i++) {
				if (validate) {
					if (!validateQuestion(children.eq(i), group, inputs)) {
						return false;
					}
				}
			}
        } else {
            return Boolean(group.find('[placeholder="Other"]').val());
        }
        return isValid;
    };

    $.validator.addMethod("matchgeo", function (value, element) {
        var $this = $(element);
        var isValid = $this.val() && !$this.hasClass('nomatch');
        /*if (!isValid) {
            callAsync(function () {
                $('#State').addClass('input-validation-error');
            });
        }*/
        $this.removeClass('nomatch');
        return isValid;
    }, "The zip code does not match the State");

    $.validator.addMethod("validzipcode", function (value, element) {
        var $this = $(element);
        var isValid = $this.val() && !$this.hasClass('notfound');
        $this.removeClass('notfound');
        return isValid;

    }, "The zip code appears to be invalid");

    var showValidationGroup = function (input, group, isValid) {
        if (isValid) {
            group.removeClass('input-validation-error')
                 .find('.input-validation-error')
                 .removeClass('input-validation-error');
        } else if (!input.closest('form').hasClass('ignore')) {
            group.addClass('input-validation-error');
        }
        return isValid;
    };

    $.validator.addMethod("requiredQuestion", function (value, element) {
        var $this = $(element);
        var group = $this.closest('.question-group');
        var inputs = group.find('.requiredQuestion:inputControls');
        var $elm;
        var isValid = false;
        for (var i = 0, l = inputs.length; i < l; i++) {
            $elm = inputs.eq(i);
            isValid = validateQuestion($elm, group, inputs);
            if (isValid) { break; }
        }
        return showValidationGroup($this, group, isValid);
    }, "Answer is required for question {num}.");

    $.fn.required = function (required) {
        if (required === false) {
            this.removeClass('required').rules('remove', 'required');
            this.valid();
        } else {
            this.addClass('required').rules('add', 'required');
        }
        return this;
	};

	$.fn.disabled = function (disable) {
		this.each(function () {
			var $this = $(this);
			if (disable === false) {
				$this.removeClass('disabled').removeAttr('disabled');
			} else {
				$this.addClass('disabled').attr('disabled', true);
			}
		});
		if (disable !== true) {
			this.filter(':first').focus();
		}
		return this;
	};

    $.fn.datepicker.Constructor.prototype.disable = function (disable) {
        var element = this.element;
        var addon = element.find('.input-group-addon');
        if (addon.exists()) {
            var click;
            if (null == (click = addon.data('clickHandler'))) {
                click = $._data(addon[0], 'events').click[0];
                if (click) { addon.data('clickHandler', click); }
            }
            var input = element.find('input.form-control');
            if (disable) {
                input.prop('disabled', true);
                addon.off('click');
            } else {
                input.prop('disabled', false);
                addon.on('click', addon.data('clickHandler'));
            }
        }
        return this;
    };

    $.fn.refreshValidation = function () {
        this.removeData("validator").removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse(this);
        return this;
    };

    $.fn.toggleAttr = function (attrName, attrValue1, attrValue2) {
        if (this.attr(attrName) === attrValue2) {
            return this.prop(attrName, attrValue1);
        } else {
            return this.prop(attrName, attrValue2);
        }
    };

    $.fn.setFocusFirstInput = function (delay) {
        var self = this;
        window.setTimeout(function() {
            self.find(':inputControls:enabled:text:not([readonly]):first').focus().select();
        }, delay || 200)
        return this;
    };

    $.fn.scollBottom = function() {
        var height = this.prop('scrollHeight');
        if (height > 0) { this.scrollTop(height); }
        return this;
    };

    $.fn.showIf = function(expression) {
        if (expression) {
            return this.removeClass('hidden');
        } else {
            return this.addClass('hidden');
        }
    };

    $.fn.postForm = function (url, callback, additionalData) {
        if (this.valid()) {
            if (typeof callback === 'boolean') {
                additionalData = callback;
                callback = null;
            }
            if (typeof additionalData === 'boolean') {
                if (additionalData === true) { this.addClass('runQuiet'); }
                additionalData = null;
            }
            var data = $.extend({}, this.toJSON(), additionalData);
            $.ajaxRequest(url, data, callback);
        }
        return this;
    };

    $.fn.clear = function () {
        this.each(function () {
            var $this = $(this);
            if ($this.is('input:checkbox, input:radio')) {
                $this.prop('checked', false);
            } else if ($this.is('input, textarea, select')) {
                $this.val('');
            }
        });
        return this;
    };

    $.fn.toJSON = function () {
        var o = {};
        var a = this.serializeArray();
        var n, i, p, m;
        $.each(a, function () {
            if (/\./i.test(this.name)) {
                var values = this.name.split('.');
                if (values.length === 2) {
                    if (m = (/(\w+)(\[(\d+)\])?\.(\w+)/i.exec(this.name))) {
                        n = m[1];
                        i = +m[3];
                        p = m[4];
                        if (o[n] === undefined) {
                            if (isNaN(i)) { o[n] = {}; }
                            else { o[n] = []; }
                        }

                        if (isNaN(i)) {
                            o[n][p] = this.value || '';
                        } else {
                            if (o[n][i] === undefined) { o[n].push({}); }
                            o[n][i][p] = this.value || '';
                        }
                    }
                }
            } else if (o[this.name] !== undefined) {
                if (!o[this.name].push) {
                    o[this.name] = [o[this.name]];
                }
                o[this.name].push(this.value || '');
            } else {
                o[this.name] = this.value || '';
            }
        });
        return o;
    };
    
    $.extend({
        ajaxRequest: function (webMethod, params, onSuccess, AJAXoptions) {
            // Extend our default options with those provided.
            // Note that the first arg to extend is an empty object -
            // this is to keep from updating our "defaults" object.
            var opts = $.extend({}, $.ajaxRequest.defaults, AJAXoptions);

            opts = $.extend(opts, { url: opts.baseurl + webMethod, data: JSON.stringify(params || {}) });
                
            if (onSuccess && (typeof onSuccess == 'function')) {
                opts.success = onSuccess;
            }
            else if (!opts.success) { opts.success = function () { }; }

            return $.ajax(opts);
        }
    });

    $.ajaxRequest.defaults = {
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        baseurl: '/ajax/',
        dataType: 'json',
        cache: false,
        async: true,
        context: document.body,
        success: function () { },
        complete: function (XMLHttpRequest, textStatus) { delete XMLHttpRequest['onreadystatechange']; }, // [KB] Stop mem leak in 1.4.2 and IE8.
        error: function () {
            if (arguments.length === 0) { return; }
            var requestObj = arguments[0];
            if (requestObj.length) {
                requestObj = requestObj[0];
            }
            var errPrefix = 'Error Calling WebMethod: "' + this.url + '"\r\n\r\n';
            try {
                var errInfo = eval("(" + requestObj.responseText + ")");
                alert(errPrefix + errInfo.Message);
            }
            catch (ex) {
                var errMsg = arguments[2];
                if (requestObj.responseText) {
                    $("html").css("overflow", "visible");
                    $("body").css({ "overflow": "visible" }).html(requestObj.responseText);
                }
                else if (errMsg && (typeof errMsg === 'string')) {
                    alert(errPrefix + arguments[2]);
                }
                else if (requestObj.statusText) {
                    alert(errPrefix + requestObj.statusText);
                }
                else {
                    alert('Unknown ' + errPrefix);
                }
            }
        }
    };
})(jQuery);