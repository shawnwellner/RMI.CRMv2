(function ($) {
    window.rmi = {};
    window.rmi.websocket = new function () {
        var self = this;
        var socket = $.connection.webSockets;
        this.events = {
            onTransferedChange: function () { }
        };

        this.onUpdate = function () { };

        socket.client.onTransferedChange = function (model, transfered) {
            self.events.onTransferedChange(model, transfered);
        };

        socket.client.onConnected = function (connId) {
            window.currentUser.connectionId = connId;
            socket.server.currentUser(connId, JSON.stringify(window.currentUser));
        };

        this.setTransfered = function (form, transfered) {
            socket.server.setTransfered(form, transfered);
        };

        $.connection.hub.stateChanged(function (state) {
            if($.connection.hub.state === 0) {
                window.currentUser.connectionId = null;
            }
        });

        var connect = function (delay) {
            setTimeout(function () {
                try {
                    $.connection.hub.start();
                } catch (ex) {
                    //Ignore Errors
                }
            }, delay || 0); // Restart connection after 5 seconds.
        };

        $.connection.hub.disconnected(function () {
            connect(5000);
        });
        connect();
    }
})(jQuery);