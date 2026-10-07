angular.module('virtoCommerce.catalogQuality')
    .controller('virtoCommerce.catalogQuality.qualityDataDetailsController', [
        '$scope', 'virtoCommerce.catalogQuality.webApi', 'platformWebApp.bladeNavigationService',
        function ($scope, api, bladeNavigationService) {
            var blade = $scope.blade;
            var formScope;

            blade.title = 'catalog-quality.blades.quality-data-details.title';
            blade.subtitle = blade.entityType + ' ' + blade.entityId;
            blade.headIcon = 'fa fa-check-square-o';
            blade.updatePermission = 'catalog:update';

            blade.refresh = function (parentRefresh) {
                blade.isLoading = true;

                api.getByEntity({ entityId: blade.entityId, entityType: blade.entityType }, function (data) {
                    blade.currentEntity = angular.copy(data);
                    blade.originalEntity = data;
                    blade.isLoading = false;

                    if (parentRefresh && blade.parentBlade.refresh) {
                        blade.parentBlade.refresh();
                    }
                }, function () {
                    blade.isLoading = false;
                });
            };

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.originalEntity) && blade.hasUpdatePermission();
            }

            function canSave() {
                return isDirty() && formScope && formScope.$valid;
            }

            function saveChanges() {
                blade.isLoading = true;

                api.update(blade.currentEntity, function () {
                    blade.refresh(true);
                }, function () {
                    blade.isLoading = false;
                });
            }

            $scope.setForm = function (form) {
                formScope = form;
            };

            blade.onClose = function (closeCallback) {
                bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, saveChanges, closeCallback,
                    'catalog-quality.dialogs.quality-data-save.title', 'catalog-quality.dialogs.quality-data-save.message');
            };

            blade.toolbarCommands = [
                //{
                //    name: 'platform.commands.save',
                //    icon: 'fa fa-save',
                //    executeMethod: saveChanges,
                //    canExecuteMethod: canSave,
                //    permission: blade.updatePermission,
                //},
                //{
                //    name: 'platform.commands.reset',
                //    icon: 'fa fa-undo',
                //    executeMethod: function () {
                //        angular.copy(blade.originalEntity, blade.currentEntity);
                //    },
                //    canExecuteMethod: isDirty,
                //    permission: blade.updatePermission,
                //},
            ];

            blade.refresh();
        }]);
