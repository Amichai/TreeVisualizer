angular.module('root', [])
.controller('homeController', ['$scope', '$http', function($scope, $http) {
    //var hub = $.connection.HomeHub;
    //$.connection.hub.start().done(function () {
    //});

    //hub.client.ConnectionEstablished = function () {
    //    console.log('connection established!');
    //}

    $scope.editor = ace.edit("editor");
    $scope.editor.getSession().setMode("ace/mode/csharp");
    $scope.editor.showInvisibles = false;
    $scope.editor.setOptions({
        enableBasicAutocompletion: true
    });

    function getParameterByName(name) {
        name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
        var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
            results = regex.exec(location.search);
        return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
    }

    $scope.type = getParameterByName("type");

    $scope.savePresentation = function () {
        $http.post(baseUrl + 'api/homeapi/save?type=' +
            encodeURIComponent($scope.type), $scope.editor.getValue()).success(function (result) {
        });
    }

    $scope.keypress = function (ev, editor) {
        if (ev.which == 13) {
            //Get highlighted text to post
            var selected = $scope.editor.getCopyText();
            //cancelBubble(ev);
            $scope.editor.insert(selected);
            var lineNumber = $scope.editor.getCursorPosition().row;
            if (selected === undefined || selected.length == 0) {
                selected = $scope.editor.getValue().split('\n')[lineNumber];
            }
            $http.post(baseUrl + 'api/homeapi/execute?lineNumber=' + lineNumber, selected).success(function (d) {
                //$scope.editor.insert(d.substring(1, d.length - 2));
                //$scope.editor.insert(new String(d));
                //$scope.editor.insert(new String(d.substring(1, d.length - 2)));
                if (d == '""') {
                    return;
                }
                if (d[0] == '"' && d[d.length - 1] == '"') {
                    d = d.substring(1, d.length - 1);
                }
                var lines = d.split('\\r\\n');
                lines = Enumerable.From(lines).SelectMany(function (i) { return i   .split('\\n') }).ToArray();
                for (var i = 0; i < lines.length; i++) {
                    $scope.editor.insert(lines[i] + "\n");
                }


            });
            ///Post to execute
        }
    }

    function cancelBubble(e) {
        var evt = e ? e : window.event;
        if (evt.stopPropagation) evt.stopPropagation();
        if (evt.cancelBubble != null) evt.cancelBubble = true;
    }
}]);