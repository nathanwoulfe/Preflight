(() => {

    function notificationController($rootScope, notificationsService, editorState) {

        this.saveCancelled = +notificationsService.current[0].args.saveCancelled === 1;

        this.switch = n => {
            $rootScope.$emit('showPreflight', { nodeId: editorState.current.id });
            this.discard(n);
        };

        this.discard = n => {
            notificationsService.remove(n);
        };

    }

    // register controller 
    angular.module('preflight').controller('preflight.notification.controller', ['$rootScope', 'notificationsService', 'editorState', notificationController]);
})();