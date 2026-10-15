angular.module('virtoCommerce.catalogQuality')
    .controller('virtoCommerce.catalogQuality.qualityTargetDetailsController', [
        '$scope', 'virtoCommerce.catalogQuality.webApi', 'platformWebApp.bladeNavigationService',
        function ($scope, api, bladeNavigationService) {
            var blade = $scope.blade;
            var formScope;

            blade.headIcon = 'fa fa-bullseye';
            blade.updatePermission = 'catalog-quality:update';

            function initialize(entity) {
                blade.currentEntity = angular.copy(entity);
                blade.originalEntity = entity;
                blade.title = blade.isNew ? 'catalog-quality.blades.quality-target-details.title-new' : entity.name;
                blade.subtitle = blade.isNew ? undefined : 'catalog-quality.blades.quality-target-details.subtitle';
                blade.isLoading = false;
            }

            blade.refresh = function (parentRefresh) {
                if (blade.isNew) {
                    initialize({});
                    return;
                }

                blade.isLoading = true;

                api.targetSearch({ objectIds: [blade.currentEntityId], take: 1 }, function (data) {
                    initialize(data.results[0]);

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

                api.targetUpdate(blade.currentEntity, function () {
                    if (blade.isNew) {
                        // Saved: mark as not dirty, otherwise closing the blade asks to save again
                        blade.originalEntity = angular.copy(blade.currentEntity);

                        // The update endpoint returns no id, so a new target is shown from the refreshed list
                        blade.parentBlade.refresh();
                        if (!isClosing) {
                            bladeNavigationService.closeBlade(blade);
                        }
                    } else {
                        blade.refresh(true);
                    }
                }, function (error) {
                    bladeNavigationService.setError('Error ' + error.status, blade);
                });
            }

            $scope.setForm = function (form) {
                formScope = form;
            };

            // Set when the blade is being closed by the platform (e.g. "Yes" in the save confirmation),
            // so saveChanges does not close it a second time
            var isClosing = false;

            blade.onClose = function (closeCallback) {
                var close = function () {
                    isClosing = true;
                    closeCallback();
                };

                bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, saveChanges, close,
                    'catalog-quality.dialogs.quality-target-save.title', 'catalog-quality.dialogs.quality-target-save.message');
            };

            blade.toolbarCommands = [
                {
                    name: 'platform.commands.save',
                    icon: 'fas fa-save',
                    executeMethod: saveChanges,
                    canExecuteMethod: canSave,
                    permission: blade.updatePermission,
                },
                {
                    name: 'platform.commands.reset',
                    icon: 'fa fa-undo',
                    executeMethod: function () {
                        angular.copy(blade.originalEntity, blade.currentEntity);
                    },
                    canExecuteMethod: isDirty,
                    permission: blade.updatePermission,
                },
            ];

            blade.refresh();
        }]);
