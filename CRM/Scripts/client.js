(function ($) {
    angular.module("client", [])
    .controller('search', function ($scope) {
        window.getClientTable = function (data) {
            if (!data) { return; }
            if (typeof data == 'object') {
                $scope.ListOfClients = data.length > 0 ? data : null;
                $scope.$apply();
            } else {
                handleResponse(data);
            }
        };

        $scope.toggleLogins = function (client, e) {
            var $this = $(e.target).toggleClass('glyphicon-plus glyphicon-minus');
            var $row = $this.closest('tr');
            if ($this.hasClass('glyphicon-plus')) {
                $row.parent().find('tr.child[parentId="{0}"]'.format(client.ClientId)).remove();
            } else {
                var childRow = "  \<tr parentId='{4}' class='child bg-warning'><td colspan='9'> \
                                    <span>{0}</span><span>{2}, {1}</span><span>{3}</span> \
                                </td></tr>";
                if (client.LoginUsers.length) {
                    var row, child;
                    for (var i = 0, l = client.LoginUsers.length; i < l; i++) {
                        c = client.LoginUsers[i];
                        $(childRow.format(c.UserId, c.FirstName, c.LastName, c.Enabled ? 'Enabled' : 'Disabled', client.ClientId)).insertAfter($row);
                    }
                }
            }
        };

        $scope.formatAddress = function (user) {
            var address;
            if (!user.City && !user.StateAbbr) {
                address = this.ZipCode || '';
            } else if (user.StateAbbr && !user.City) {
                address = "{0} {1}".format(user.StateAbbr, user.ZipCode || '');
            } else {
                address = "{0},{1} {2}".format(user.City || '', user.StateAbbr || '', user.ZipCode || '');
            }
            return address;
        };

    });

    $(document).on('click', '#AddZipCode', function () {
        var template = $($('#zipPrefixTemplate').html());
        var $this = $(this);
        var form = $(this).closest('form');
        if (form.valid()) {
            var zipCount = $('.ziplist .zpbx').length;
            var field;
            //var input = $('.zpbx:last');
            template.clone().insertAfter('.zpbx:last').find('input').each(function () {
                $this = $(this);
                prop = $this.is(':hidden') ? "Key" : "Value";
                if ($this.is(':hidden')) {
                    field = "Key";
                    $this.attr('data-val-required', "The {0} field is required.".format(field))
                         .attr('data-val-number', "The field {0} must be a number.".format(field))
                         .attr('data-val', "true".format(zipCount));
                } else {
                    field = "Value";
                    $this.attr('data-required', "true")
                        .attr('aria-describedby', "ListOfZipCodes_{0}__Value-error".format(zipCount))
                        .attr('data-placement', "top")
                        .attr('data-content', 'Click to remove zip');

                }
                this.value = '';
                this.id = 'ListOfZipCodes_{0}__{1}'.format(zipCount, field);
                this.name = 'ListOfZipCodes[{0}].{1}'.format(zipCount, field);

            });
            $("[rel=popover]").popover({ 'trigger': 'hover' });
            form.refreshValidation();
        }
    }).on('click', '#RemoveZipCode', function () {
        if ($(".zpbx").length > 1) {
            this.closest('.zpbx').remove();
        }
    }).on('change', 'input.zipbox', function (e) {
        var cache = this;
        window.ValidateZip(cache.value, function (results) {
            if (results.indexOf("invalid") > -1) {
                $(cache).addClass("input-validation-error");
            } else {
                $(cache).removeClass("input-validation-error");
            }
            $(cache).closest('.zpbx').find('label').html(results);
        });
    }).on('click', '#addUserLogin', function (e) { //Add Client Login
        var $this = $(this);
        var form = $(this).closest('form');
        if (form.valid()) {
            var params = form.serialize();
            $.post('/ajax/AddClientLoginUser', params, function (results) {
                form.find('#clientLogins').prepend(results)
                    .find(':inputControls:first').focus();
                form.refreshValidation();
            });
        }
    });

    var ValidateZip = function (zip, callback) {
        $('form').addClass('runQuiet');
        $.ajax({
            url: "http://api.zippopotam.us/us/" + zip,
            cache: false,
            async: true,
            traditional: true,
            type: "GET",
            success: function (response) {
                var jsonObj = response;
                if (jsonObj["places"].length > 0) {
                    var returnstring = jsonObj['places'][0]['place name'] + " " + jsonObj['places'][0]['state abbreviation'];
                    callback(returnstring);
                }
            },
            error: function (xhr) {
                callback("invalid");
            }
        });

    };

    var GetTargetModel = function (dropdown) {
        var currentTarget = +$(dropdown).find(':checked').val();
        if (currentTarget === 3) {
            var params = { clientId: +$('#UserId').val() };

            $.post("/ajax/getzipcodeprefix/", params)
                .done(function (response) {
                    $("#targetresp").html(response);
                    $("[rel=popover]").popover({ 'trigger': 'hover' });
                });
        } else {
            $("#targetresp").html("");
        }
    };

    var getLocationSuccess = function (data) {
        //xmlDoc = $.parseXML( data )
        //alert(data);
        if (data.indexOf("field-validation-error") <= 0) {
            $("#zipcodeContainer").css("display", "none");
            $(".overlayStep2").css("display", "none");
            $(".overlayStep3").css("display", "none");
            $("#location").html(data.substring(0, data.indexOf("DD-", 0)));
            $("#directionsxml").html(data.substring(data.indexOf("DD-", 0), data.indexOf("-XML-", 0)).replace("DD-", ""));
            $("#timesxml").html(data.substring(data.indexOf("-XML-", 0)).replace("-XML-", ""));
            $("#location").css("display", "block");
        } else {
            $("#ZipError").html(data);
            $("#Zip").css("class", "textbox input-validation-error");
        }
    };
})(jQuery);