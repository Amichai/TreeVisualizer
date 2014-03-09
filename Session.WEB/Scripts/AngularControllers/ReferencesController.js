angular.module('root', [])
.controller('referencesController', ['$scope', '$http', function ($scope, $http) {
    $http.get(baseUrl + 'api/referencesAPI/getreferences').success(function (result) {
        $scope.references = result;
    });

    $http.get(baseUrl + 'api/referencesAPI/getnamespaces').success(function (result) {
        $scope.namespaces = result;
    });

    $scope.importNamespace = function(){
        $http.post(baseUrl + 'api/referencesAPI/importnamespace?toImport=' + $scope.newNamespace).success(function () {
            debugger;
        });
    }
}]);