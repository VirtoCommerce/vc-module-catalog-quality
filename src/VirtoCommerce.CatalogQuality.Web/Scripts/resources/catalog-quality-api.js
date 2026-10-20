angular.module('virtoCommerce.catalogQuality')
    .factory('virtoCommerce.catalogQuality.webApi', ['$resource', function ($resource) {
        return $resource('api/catalogquality', {}, {
            dataSearch: { method: 'POST', url: 'api/catalogquality/data/search' },
            dataGetByEntity: { method: 'GET', url: 'api/catalogquality/data/getbyentity' },
            dataUpdate: { method: 'POST', url: 'api/catalogquality/data/update' },
            dataBulkUpdate: { method: 'POST', url: 'api/catalogquality/data/bulkupdate' },
            targetSearch: { method: 'POST', url: 'api/catalogquality/target/search' },
            targetUpdate: { method: 'POST', url: 'api/catalogquality/target/update' },
            targetDelete: { method: 'DELETE', url: 'api/catalogquality/target/delete' },
        });
    }]);
