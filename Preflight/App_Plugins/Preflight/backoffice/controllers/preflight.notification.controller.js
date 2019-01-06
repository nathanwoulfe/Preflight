(() => {

    function notificationController(notificationsService) {

        this.saveCancelled = notificationsService.current[0].args.saveCancelled == 1;

        this.switch = n => {
            const tabScope = angular
                .element(document.querySelector('[data-element="sub-view-preflight"]')).scope();

            if (tabScope) {
                tabScope.$parent.clickNavigationItem(tabScope.item);
            }

            this.discard(n);
        };

        this.discard = n => {
            notificationsService.remove(n);
        };

    }

    // register controller 
    angular.module('umbraco').controller('preflight.notification.controller', ['notificationsService', notificationController]);
})();