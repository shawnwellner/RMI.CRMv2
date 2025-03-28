(function ($) {
    angular.module("callcenter", [])
    .controller('search', function ($scope) {
        window.getCallCenterTable = function (data) {
            if (!data) { return; }
            if (typeof data == 'object') {
                $scope.ListOfUsers = data.length > 0 ? data : null;
                $scope.$apply();
            } else {
                handleResponse(data);
            }
        };
    }).controller('update', function ($scope) {
        $scope.role = "";
        $scope.$watch('role', function (value) {
            $scope.role = value;
            //$('.vertical-list input:not(".requiredQuestion")').addClass('requiredQuestion').prop('checked', false);
            if (value == 2) {
                var checkboxes = $('.vertical-list input:checkbox').toggleClass('requiredQuestion');
                if (!checkboxes.filter(':checked').exists()) {
                    checkboxes.filter(':first').prop('checked', true);
                }
            }
        });
    });

    $(document).ready(function () {
        var required = $('#Enabled:checked').length === 1;
        $('select#Five9AgentId').required(required);
    }).on('change', 'select#Five9AgentId', function () { //Auto fill Five9 Agent information when selecting dropdown
        var info = $(this).find(':selected').attr('five9Item');
        if (info) {
            var user = JSON.parse(info);
            $('#FirstName').val(user.FirstName).valid();
            $('#LastName').val(user.LastName).valid();
            $('#Email').val(user.Email).valid();
            if (/responsemine/i.test(user.Email)) {
                $("#Phone").val('4042330370').valid();
            } else {
                $("#Phone").focus();
            }
        } /*else {
            $('#LastName').val("");
            $('#Phone').val("");
            $('#Email').val("");
            $('#FirstName').val("").focus();
        }*/
    }).on('change', '#Enabled', function () {
        var required = $('#Enabled:checked').length === 1;
        $('select#Five9AgentId').required(required);
    });
})(jQuery);