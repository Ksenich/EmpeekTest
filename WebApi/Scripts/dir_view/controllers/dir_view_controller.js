angular.module("dirViewApp")
    .controller("dir_view", ['$scope', 'Dir', function ($scope, Dir) {
        $scope.path = "";
        $scope.open = function (p) {

            Dir.get({path: p || $scope.path }, function (data) {
                $scope.dir = data;
            }, function (err) {

            });

        };
        $scope.open();
    }]);