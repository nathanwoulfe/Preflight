export class NotificationController {

    static controllerName = 'preflight.notification.controller';

    $rootScope;
    notificationsService;
    editorState;

    saveCancelled;

    constructor($rootScope, notificationsService, editorState) {
        this.$rootScope = $rootScope;
        this.notificationsService = notificationsService;
        this.editorState = editorState;

        this.saveCancelled = +notificationsService.current[0].args.saveCancelled === 1;
    }

    switch = n => {
        this.$rootScope.$emit('showPreflight', { nodeId: this.editorState.current.id });
        this.discard(n);
    };

    discard = n => {
        this.notificationsService.remove(n);
    };
}
