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

    $scope.resetSession = function () {
        $http.post(baseUrl + 'api/homeapi/resetSession').success(function (result) {
            if (result) {
            } else {
            }
        });
    }

    $scope.clearResults = function () {
        $('#resultArea').html('');
    };

    function htmlEncode(value) {
        //create a in-memory div, set it's inner text(which jQuery automatically encodes)
        //then grab the encoded contents back out.  The div never exists on the page.
        return $('<div/>').text(value).html();
    }

    function htmlDecode(value) {
        return $('<div/>').html(value).text();
    }

    $scope.keypress = function (ev, editor) {
        if (ev.which == 13) {
            //Get highlighted text to post
            var selected = $scope.editor.getCopyText();
            //cancelBubble(ev);
            $scope.editor.insert(selected);

            var cursorPos = $scope.editor.getCursorPosition();
            var lineNumber = cursorPos.row;
            if (selected === undefined || selected.length == 0) {
                selected = $scope.editor.getValue().split('\n')[lineNumber];
            }
            if (selected == "" && cursorPos.column != selected.length) {
                return;
            }
            $http.post(baseUrl + 'api/homeapi/execute?lineNumber=' + lineNumber, selected).success(function (result) {
                //var j = eval(result["javascript"]);
                //$('#resultArea').html(j[0]);
                //return;
                d = result["htmlresult"];

                if (d == '""' || d == '') {
                    return;
                }

                if (d[0] == '"' && d[d.length - 1] == '"') {
                    d = d.substring(1, d.length - 1);
                }
                if (result["textResult"]) {
                    $scope.editor.insert(d);
                    return;
                }
                append("<br /><b>Line number " + (lineNumber + 1) + ":</b><br />");
                append(d);
            });
        }

        function append(data) {
            $('#resultArea').append(data);
        }
    }

    function cancelBubble(e) {
        var evt = e ? e : window.event;
        if (evt.stopPropagation) evt.stopPropagation();
        if (evt.cancelBubble != null) evt.cancelBubble = true;
    }
}]);