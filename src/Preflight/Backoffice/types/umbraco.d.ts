interface AceEditorOptionsModel {
  mode: string;
  theme: string;
  showPrintMargin: boolean;
  advanced: {
    fontSize: string;
  };
  onLoad: Function;
}

interface UmbNavigation {
  name: string;
  alias: string;
  icon: string;
  view: string;
  active?: boolean
}

interface UmbButton {
  labelKey: string;
  buttonStyle: string;
  handler: () => void;
  shortcut: string;
}

interface UmbUser {
  id: number;
  name: string;
  email: string;
  locale: string;
  allowedSections: Array<string>;
}

interface UmbUserGroup {
  groupId: number;
  users: Array<UmbUser>;
}

interface UmbNode {
  id: number;
  path: string;
  variants: Array<UmbVariant>;
  updateDate: string;
  notifications: Array<UmbNotification>;
  isChildOfListView: boolean;
}

interface UmbNotification {

}

interface UmbVariant {
  active?: boolean;
  language: UmbLanguage;
  publishDate?: string;
}

interface UmbLanguage {
  culture: string;
  name: string;
  isDefault?: boolean;
}

interface UmbOverlayModelBase {
  submit?: (model: any) => void;
  close: () => void;
}

interface UmbOverlayModel extends UmbOverlayModelBase {
  [key: string]: any;

  view?: string;
  size?: string;
  title?: string;
  description?: string;

  submitButtonLabelKey?: string;
  submitButtonStyle?: string;

  hideHeader?: boolean;
  hideSubmitButton?: boolean;
  hideDescription?: boolean;
}

interface UmbUserPickerOverlayModel extends UmbOverlayModel {
  selection: Array<UmbUser | WorkflowUser>;
}

interface UmbUserGroupPickerOverlayModel extends UmbOverlayModel {
  selection: Array<UmbUserGroup | WorkflowUserGroup>;
}

interface UmbItemPickerOverlayModel extends UmbOverlayModel {
  selection: Array<any>;
}

interface UmbMediaPickerOverlayModel extends UmbOverlayModel {
  selection: Array<any>;
}

interface FormHelper {
  submitForm: (arg: { scope: object, action?: string }) => boolean;
  resetForm: (arg: { scope: object }) => void;
}

interface DateHelper {
  convertToServerStringTime: (date: any, offset: string) => any;
  getLocalDate: (date: string | Date | null, locale: string, format: string) => any;
}

interface RequestHelper {
  resourcePromise: (method: unknown, errorMessage: string) => any;
}

interface ContentEditingHelper {
  contentEditorPerformSave: (args: {}) => Promise<any>;
}

/// Services - Umbraco

interface ILocalizationService {
  localizeMany: (keyArray: Array<string>) => Promise<Array<string>>;
  localize: (key: string, tokens?: Array<string>) => Promise<string>;
}

interface ILanguageResource {
  getAll: () => Promise<Array<UmbLanguage>>;
}

interface IEditorService {
  open: (model: UmbOverlayModel) => void;
  close: () => void;
  closeAll: () => void;
  userPicker: (model: UmbUserPickerOverlayModel) => void;
  userGroupPicker: (model: UmbUserGroupPickerOverlayModel) => void;
  itemPicker: (model: UmbItemPickerOverlayModel) => void;
  mediaPicker: (model: UmbMediaPickerOverlayModel) => void;
}

interface IOverlayService {
  open: (model: UmbOverlayModel) => void;
  close: () => void;
}

interface INavigationService {
  changeSection: (section: string) => void;
  syncTree: (args: { tree: string, path: Array<number>, forceReload?: boolean, activate?: boolean }) => Promise<any>;
  clearSearch: (args: string[]) => void;
  setSoftRedirect: () => void;
}

interface IUserGroupsService {
  getUserGroups: () => Promise<Array<any>>;
}

interface IAuthResource {
  getCurrentUser: () => Promise<UmbUser>;
}

interface IAssetsService {
  loadJs: (path: string) => Promise<any>;
  loadCss: (path: string, scope: any) => Promise<any>;
}

interface INotificationsService {
  error: (header: string, message: string) => void;
  warning: (message: string) => void;
  add: (notification: UmbNotification) => void;
}

interface IContentResource {
  save: (content: any, isNew: boolean, files: Array<any>, somethingElse: boolean) => Promise<UmbNode>;
}
