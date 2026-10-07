angular.module('virtoCommerce.catalogQuality')
    .factory('virtoCommerce.catalogQuality.webApi', ['$resource', function ($resource) {
        return $resource('api/catalogquality', {}, {
            search: { method: 'POST', url: 'api/catalogquality/search' },
            getByEntity: { method: 'GET', url: 'api/catalogquality/getbyentity' },
            update: { method: 'POST', url: 'api/catalogquality/update' },
        });
    }]);
