angular.module('virtoCommerce.catalogQuality')
    .controller('virtoCommerce.catalogQuality.qualityTargetDetailsController', [
        '$scope', 'virtoCommerce.catalogQuality.webApi', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'virtoCommerce.catalogQuality.qualityDataTypes',
        function ($scope, api, bladeNavigationService, dialogService, qualityDataTypes) {
            var blade = $scope.blade;
            var formScope;

            blade.headIcon = 'fa fa-bullseye';
            blade.updatePermission = 'catalog-quality:update';

            function initialize(entity) {
                blade.currentEntity = angular.copy(entity);
                blade.originalEntity = entity;
                blade.title = blade.isNew ? 'catalog-quality.blades.quality-target-details.title-new' : entity.name;
                blade.subtitle = blade.isNew ? undefined : 'catalog-quality.blades.quality-target-details.subtitle';
                blade.qualityDataRegisteredTypes = qualityDataTypes.getAllTypes();
                blade.isLoading = false;
            }

            blade.refresh = function (parentRefresh) {
                if (blade.isNew) {
                    initialize({});
                    return;
                }

                blade.isLoading = true;

                api.targetGetById({ id: blade.currentEntityId }, function (data) {
                    initialize(data);

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

            function deleteTarget() {
                dialogService.showConfirmationDialog({
                    id: 'confirmDeleteQualityTarget',
                    title: 'catalog-quality.dialogs.quality-target-delete-one.title',
                    message: 'catalog-quality.dialogs.quality-target-delete-one.message',
                    messageValues: { name: blade.originalEntity.name },
                    callback: function (remove) {
                        if (remove) {
                            blade.isLoading = true;

                            api.targetDelete({ ids: [blade.currentEntityId] }, function () {
                                // Deleted: drop unsaved changes, otherwise closing the blade asks to save them
                                angular.copy(blade.originalEntity, blade.currentEntity);

                                blade.parentBlade.refresh();
                                bladeNavigationService.closeBlade(blade);
                            }, function (error) {
                                bladeNavigationService.setError('Error ' + error.status, blade);
                            });
                        }
                    },
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

            // A new target is not saved yet, so there is nothing to delete
            if (!blade.isNew) {
                blade.toolbarCommands.push({
                    name: 'platform.commands.delete',
                    icon: 'fas fa-trash-alt',
                    executeMethod: deleteTarget,
                    canExecuteMethod: function () { return !blade.isLoading; },
                    permission: 'catalog-quality:delete',
                });
            }

            blade.refresh();
        }]);
