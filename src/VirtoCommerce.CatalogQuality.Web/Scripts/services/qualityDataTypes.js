angular.module('virtoCommerce.catalogQuality')
    .factory('virtoCommerce.catalogQuality.qualityDataTypes', [function () {
        var registeredTypes = [];

        return {
            addType: function (entityType) {
                registeredTypes.push(entityType);
            },
            getAllTypes: function () {
                return registeredTypes;
            },
            getTypeInfo: function (type) {
                return registeredTypes.find(x => x.value === type);
            }
        };
    }]);
