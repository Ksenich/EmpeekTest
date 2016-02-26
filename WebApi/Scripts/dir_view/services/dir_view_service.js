angular.module("dirViewApp")
    .factory("Dir", ['$resource', function ($resource) {
        return $resource("/api/values");
    }]);