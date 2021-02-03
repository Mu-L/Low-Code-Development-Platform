
import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { Prompt, Redirect } from 'react-router';
import { Button, Row, Col, ButtonToolbar, ButtonGroup, DropdownItem, DropdownMenu, DropdownToggle, UncontrolledDropdown, Nav, NavItem, NavLink } from 'reactstrap';
import { Formik, Field, Form } from 'formik';
import DocumentTitle from '../../components/custom/DocumentTitle';
import classNames from 'classnames';
import LoadingIcon from 'mdi-react/LoadingIcon';
import CheckIcon from 'mdi-react/CheckIcon';
import DatePicker from '../../components/custom/DatePicker';
import NaviBar from '../../components/custom/NaviBar';
import DropdownField from '../../components/custom/DropdownField';
import AutoCompleteField from '../../components/custom/AutoCompleteField';
import ListBox from '../../components/custom/ListBox';
import { default as FileInputFieldV1 } from '../../components/custom/FileInputV1';
import { default as FileInputField } from '../../components/custom/FileInput';
import SignaturePanel from '../../components/custom/SignaturePanel';
import RintagiScreen from '../../components/custom/Screen';
import ModalDialog from '../../components/custom/ModalDialog';
import { showNotification } from '../../redux/Notification';
import { registerBlocker, unregisterBlocker } from '../../helpers/navigation'
import { isEmptyId, getAddDtlPath, getAddMstPath, getEditDtlPath, getEditMstPath, getNaviPath, getDefaultPath, decodeEmbeddedFileObjectFromServer } from '../../helpers/utils'
import { toMoney, toLocalAmountFormat, toLocalDateFormat, toDate, strFormat, formatContent } from '../../helpers/formatter';
import { setTitle, setSpinner } from '../../redux/Global';
import { RememberCurrent, GetCurrent } from '../../redux/Persist'
import { getNaviBar } from './index';
import AdmUsrImprReduxObj, { ShowMstFilterApplied } from '../../redux/AdmUsrImpr';
import * as AdmUsrImprService from '../../services/AdmUsrImprService';
import { getRintagiConfig } from '../../helpers/config';
import Skeleton from 'react-skeleton-loader';
import ControlledPopover from '../../components/custom/ControlledPopover';
import log from '../../helpers/logger';

class MstRecord extends RintagiScreen {
  constructor(props) {
    super(props);
    this.GetReduxState = () => (this.props.AdmUsrImpr || {});
    this.blocker = null;
    this.titleSet = false;
    this.MstKeyColumnName = 'UsrImprId95';
    this.SystemName = (document.Rintagi || {}).systemName || 'Rintagi';
    this.confirmUnload = this.confirmUnload.bind(this);
    this.hasChangedContent = false;
    this.setDirtyFlag = this.setDirtyFlag.bind(this);
    this.AutoCompleteFilterBy = (option, props) => { return true };
    this.OnModalReturn = this.OnModalReturn.bind(this);
    this.ValidatePage = this.ValidatePage.bind(this);
    this.SavePage = this.SavePage.bind(this);
    this.FieldChange = this.FieldChange.bind(this);
    this.DateChange = this.DateChange.bind(this);
    this.StripEmbeddedBase64Prefix = this.StripEmbeddedBase64Prefix.bind(this);
    this.DropdownChangeV1 = this.DropdownChangeV1.bind(this);
    this.FileUploadChangeV1 = this.FileUploadChangeV1.bind(this);
    this.mobileView = window.matchMedia('(max-width: 1200px)');
    this.mediaqueryresponse = this.mediaqueryresponse.bind(this);
    this.SubmitForm = ((submitForm, options = {}) => {
      const _this = this;
      return (evt) => {
        submitForm();
      }
    }
    );
    this.state = {
      submitting: false,
      ScreenButton: null,
      key: '',
      Buttons: {},
      ModalColor: '',
      ModalTitle: '',
      ModalMsg: '',
      ModalOpen: false,
      ModalSuccess: null,
      ModalCancel: null,
      isMobile: false,
    }
    if (!this.props.suppressLoadPage && this.props.history) {
      RememberCurrent('LastAppUrl', (this.props.history || {}).location, true);
    }

    if (!this.props.suppressLoadPage) {
      this.props.setSpinner(true);
    }
  }

  mediaqueryresponse(value) {
    if (value.matches) { // if media query matches
      this.setState({ isMobile: true });
    }
    else {
      this.setState({ isMobile: false });
    }
  }

  UsrId95InputChange() { const _this = this; return function (name, v) { const filterBy = ''; _this.props.SearchUsrId95(v, filterBy); } }
  UsrId95Change(v, name, values, { setFieldValue, setFieldTouched, forName, _this, blur } = {}) {
    const key = (v || {}).key || v;
    const mstId = (values.cUsrImprId95 || {}).key || values.cUsrImprId95;
    (this || _this).props.GetRefUsrId95(mstId, null, key, null)
      .then(ret => {
        ret.dependents.forEach(
          (o => {
            setFieldValue('c' + o.columnName, !ret.result ? null : o.isFileObject ? decodeEmbeddedFileObjectFromServer(ret.result[o.tableColumnName], true) : ret.result[o.tableColumnName]);
          })
        )
      })
  }

  ImprUsrId95InputChange() { const _this = this; return function (name, v) { const filterBy = ''; _this.props.SearchImprUsrId95(v, filterBy); } }
  ImprUsrId95Change(v, name, values, { setFieldValue, setFieldTouched, forName, _this, blur } = {}) {
    const key = (v || {}).key || v;
    const mstId = (values.cUsrImprId95 || {}).key || values.cUsrImprId95;
    (this || _this).props.GetRefImprUsrId95(mstId, null, key, null)
      .then(ret => {
        ret.dependents.forEach(
          (o => {
            setFieldValue('c' + o.columnName, !ret.result ? null : o.isFileObject ? decodeEmbeddedFileObjectFromServer(ret.result[o.tableColumnName], true) : ret.result[o.tableColumnName]);
          })
        )
      })
  }

  TestCulture95InputChange() { const _this = this; return function (name, v) { const filterBy = ''; _this.props.SearchTestCulture95(v, filterBy); } }
  TestCulture95Change(v, name, values, { setFieldValue, setFieldTouched, forName, _this, blur } = {}) {
    const key = (v || {}).key || v;
    const mstId = (values.cUsrImprId95 || {}).key || values.cUsrImprId95;
    (this || _this).props.GetRefTestCulture95(mstId, null, key, null)
      .then(ret => {
        ret.dependents.forEach(
          (o => {
            setFieldValue('c' + o.columnName, !ret.result ? null : o.isFileObject ? decodeEmbeddedFileObjectFromServer(ret.result[o.tableColumnName], true) : ret.result[o.tableColumnName]);
          })
        )
      })
  }

  /* ReactRule: Master Record Custom Function */

  /* ReactRule End: Master Record Custom Function */

  /* form related input handling */

  ValidatePage(values) {
    const errors = {};
    const columnLabel = (this.props.AdmUsrImpr || {}).ColumnLabel || {};
    /* standard field validation */
    if (isEmptyId((values.cUsrId95 || {}).value)) { errors.cUsrId95 = (columnLabel.UsrId95 || {}).ErrMessage; }
    if (isEmptyId((values.cImprUsrId95 || {}).value)) { errors.cImprUsrId95 = (columnLabel.ImprUsrId95 || {}).ErrMessage; }
    return errors;
  }

  SavePage(values, { setSubmitting, setErrors, resetForm, setFieldValue, setValues }) {
    const errors = [];
    const currMst = (this.props.AdmUsrImpr || {}).Mst || {};

    /* ReactRule: Master Record Save */

    /* ReactRule End: Master Record Save */

    if (errors.length > 0) {
      this.props.showNotification('E', { message: errors[0] });
      setSubmitting(false);
    }
    else {
      const { ScreenButton, OnClickColumeName } = this;
      this.setState({ submittedOn: Date.now(), submitting: true, setSubmitting: setSubmitting, key: currMst.key, ScreenButton: ScreenButton, OnClickColumeName: OnClickColumeName });
      this.ScreenButton = null;
      this.OnClickColumeName = null;
      this.props.SavePage(
        this.props.AdmUsrImpr,
        {
          UsrImprId95: values.cUsrImprId95 || '',
          UsrId95: (values.cUsrId95 || {}).value || '',
          UPicMed1: values.cUPicMed1 && values.cUPicMed1.ts ?
            JSON.stringify({
              ...values.cUPicMed1,
              ts: undefined,
              lastTS: values.cUPicMed1.ts,
              base64: this.StripEmbeddedBase64Prefix(values.cUPicMed1.base64)
            }) : null,
          ImprUsrId95: (values.cImprUsrId95 || {}).value || '',
          IPicMed1: values.cIPicMed1 && values.cIPicMed1.ts ?
            JSON.stringify({
              ...values.cIPicMed1,
              ts: undefined,
              lastTS: values.cIPicMed1.ts,
              base64: this.StripEmbeddedBase64Prefix(values.cIPicMed1.base64)
            }) : null,
          InputBy95: (values.cInputBy95 || {}).value || '',
          InputOn95: values.cInputOn95 || '',
          ModifiedBy95: (values.cModifiedBy95 || {}).value || '',
          ModifiedOn95: values.cModifiedOn95 || '',
          TestCulture95: (values.cTestCulture95 || {}).value || '',
          TestCurrency95: values.cTestCurrency95 || '',
          SignOff95: this.StripEmbeddedBase64Prefix(values.cSignOff95) || '',
        },
        [],
        {
          persist: true,
          ScreenButton: (ScreenButton || {}).buttonType,
          OnClickColumeName: OnClickColumeName,
        }
      );
    }
  }
  /* end of form related handling functions */

  /* standard screen button actions */
  SaveMst({ submitForm, ScreenButton }) {
    return function (evt) {
      this.ScreenButton = ScreenButton;
      submitForm();
    }.bind(this);
  }
  SaveCloseMst({ submitForm, ScreenButton, naviBar, redirectTo, onSuccess }) {
    return function (evt) {
      this.ScreenButton = ScreenButton;
      submitForm();
    }.bind(this);
  }
  NewSaveMst({ submitForm, ScreenButton }) {
    return function (evt) {
      this.ScreenButton = ScreenButton;
      submitForm();
    }.bind(this);
  }
  CopyHdr({ ScreenButton, mst, mstId, useMobileView }) {
    const AdmUsrImprState = this.props.AdmUsrImpr || {};
    const auxSystemLabels = AdmUsrImprState.SystemLabel || {};
    return function (evt) {
      evt.preventDefault();
      const fromMstId = mstId || (mst || {}).UsrImprId95;
      const copyFn = () => {
        if (fromMstId) {
          this.props.AddMst(fromMstId, 'MstRecord', 0);
          /* this is application specific rule as the Posted flag needs to be reset */
          this.props.AdmUsrImpr.Mst.Posted64 = 'N';
          if (useMobileView) {
            const naviBar = getNaviBar('MstRecord', {}, {}, this.props.AdmUsrImpr.Label);
            this.props.history.push(getEditMstPath(getNaviPath(naviBar, 'MstRecord', '/'), '_'));
          }
          else {
            if (this.props.onCopy) this.props.onCopy();
          }
        }
        else {
          this.setState({ ModalOpen: true, ModalColor: 'warning', ModalTitle: auxSystemLabels.UnsavedPageTitle || '', ModalMsg: auxSystemLabels.UnsavedPageMsg || '' });
        }
      }
      if (!this.hasChangedContent) copyFn();
      else this.setState({ ModalOpen: true, ModalSuccess: copyFn, ModalColor: 'warning', ModalTitle: auxSystemLabels.UnsavedPageTitle || '', ModalMsg: auxSystemLabels.UnsavedPageMsg || '' });
    }.bind(this);
  }
  DelMst({ naviBar, ScreenButton, mst, mstId }) {
    const AdmUsrImprState = this.props.AdmUsrImpr || {};
    const auxSystemLabels = AdmUsrImprState.SystemLabel || {};
    return function (evt) {
      evt.preventDefault();
      const deleteFn = () => {
        const fromMstId = mstId || mst.UsrImprId95;
        this.props.DelMst(this.props.AdmUsrImpr, fromMstId);
      };
      this.setState({ ModalOpen: true, ModalSuccess: deleteFn, ModalColor: 'danger', ModalTitle: auxSystemLabels.WarningTitle || '', ModalMsg: auxSystemLabels.DeletePageMsg || '' });
    }.bind(this);
  }
  /* end of screen button action */

  /* react related stuff */
  static getDerivedStateFromProps(nextProps, prevState) {
    const nextReduxScreenState = nextProps.AdmUsrImpr || {};
    const buttons = nextReduxScreenState.Buttons || {};
    const revisedButtonDef = super.GetScreenButtonDef(buttons, 'Mst', prevState);
    const currentKey = nextReduxScreenState.key;
    const waiting = nextReduxScreenState.page_saving || nextReduxScreenState.page_loading;
    let revisedState = {};
    if (revisedButtonDef) revisedState.Buttons = revisedButtonDef;

    if (prevState.submitting && !waiting && nextReduxScreenState.submittedOn > prevState.submittedOn) {
      prevState.setSubmitting(false);
    }

    return revisedState;
  }

  confirmUnload(message, callback) {
    const AdmUsrImprState = this.props.AdmUsrImpr || {};
    const auxSystemLabels = AdmUsrImprState.SystemLabel || {};
    const confirm = () => {
      callback(true);
    }
    const cancel = () => {
      callback(false);
    }
    this.setState({ ModalOpen: true, ModalSuccess: confirm, ModalCancel: cancel, ModalColor: 'warning', ModalTitle: auxSystemLabels.UnsavedPageTitle || '', ModalMsg: message });
  }

  setDirtyFlag(dirty) {
    /* this is called during rendering but has side-effect, undesirable but only way to pass formik dirty flag around */
    if (dirty) {
      if (this.blocker) unregisterBlocker(this.blocker);
      this.blocker = this.confirmUnload;
      registerBlocker(this.confirmUnload);
    }
    else {
      if (this.blocker) unregisterBlocker(this.blocker);
      this.blocker = null;
    }
    if (this.props.updateChangedState) this.props.updateChangedState(dirty);
    this.SetCurrentRecordState(dirty);
    return true;
  }

  componentDidMount() {
    this.mediaqueryresponse(this.mobileView);
    this.mobileView.addListener(this.mediaqueryresponse) // attach listener function to listen in on state changes
    const isMobileView = this.state.isMobile;
    const useMobileView = (isMobileView && !(this.props.user || {}).desktopView);
    const suppressLoadPage = this.props.suppressLoadPage;
    if (!suppressLoadPage) {
      const { mstId } = { ...this.props.match.params };
      if (!(this.props.AdmUsrImpr || {}).AuthCol || true) {
        this.props.LoadPage('MstRecord', { mstId: mstId || '_' });
      }
    }
    else {
      return;
    }
  }

  componentDidUpdate(prevprops, prevstates) {
    const currReduxScreenState = this.props.AdmUsrImpr || {};

    if (!this.props.suppressLoadPage) {
      if (!currReduxScreenState.page_loading && this.props.global.pageSpinner) {
        const _this = this;
        setTimeout(() => _this.props.setSpinner(false), 500);
      }
    }

    const currMst = currReduxScreenState.Mst || {};
    this.SetPageTitle(currReduxScreenState);
    if (prevstates.key !== currMst.key) {
      if ((prevstates.ScreenButton || {}).buttonType === 'SaveClose') {
        const currDtl = currReduxScreenState.EditDtl || {};
        const dtlList = (currReduxScreenState.DtlList || {}).data || [];
        const naviBar = getNaviBar('MstRecord', currMst, currDtl, currReduxScreenState.Label);
        const searchListPath = getDefaultPath(getNaviPath(naviBar, 'MstList', '/'))
        this.props.history.push(searchListPath);
      }
    }
  }

  componentWillUnmount() {
    if (this.blocker) {
      unregisterBlocker(this.blocker);
      this.blocker = null;
    }
    this.mobileView.removeListener(this.mediaqueryresponse);
  }


  render() {
    const AdmUsrImprState = this.props.AdmUsrImpr || {};

    if (AdmUsrImprState.access_denied) {
      return <Redirect to='/error' />;
    }

    const screenHlp = AdmUsrImprState.ScreenHlp;

    // Labels
    const siteTitle = (this.props.global || {}).pageTitle || '';
    const MasterRecTitle = ((screenHlp || {}).MasterRecTitle || '');
    const MasterRecSubtitle = ((screenHlp || {}).MasterRecSubtitle || '');
    const NoMasterMsg = ((screenHlp || {}).NoMasterMsg || '');

    const screenButtons = AdmUsrImprReduxObj.GetScreenButtons(AdmUsrImprState) || {};
    const itemList = AdmUsrImprState.Dtl || [];
    const auxLabels = AdmUsrImprState.Label || {};
    const auxSystemLabels = AdmUsrImprState.SystemLabel || {};

    const columnLabel = AdmUsrImprState.ColumnLabel || {};
    const authCol = this.GetAuthCol(AdmUsrImprState);
    const authRow = (AdmUsrImprState.AuthRow || [])[0] || {};
    const currMst = ((this.props.AdmUsrImpr || {}).Mst || {});
    const currDtl = ((this.props.AdmUsrImpr || {}).EditDtl || {});
    const naviBar = getNaviBar('MstRecord', currMst, currDtl, screenButtons).filter(v => ((v.type !== 'DtlRecord' && v.type !== 'DtlList') || currMst.UsrImprId95));
    const selectList = AdmUsrImprReduxObj.SearchListToSelectList(AdmUsrImprState);
    const selectedMst = (selectList || []).filter(v => v.isSelected)[0] || {};

    const UsrImprId95 = currMst.UsrImprId95;
    const UsrId95List = AdmUsrImprReduxObj.ScreenDdlSelectors.UsrId95(AdmUsrImprState);
    const UsrId95 = currMst.UsrId95;
    const UPicMed1 = currMst.UPicMed1 ? decodeEmbeddedFileObjectFromServer(currMst.UPicMed1) : null;
    const UPicMed1FileUploadOptions = {
      CancelFileButton: auxSystemLabels.CancelFileBtnLabel,
      DeleteFileButton: auxSystemLabels.DeleteFileBtnLabel,
      MaxImageSize: {
        Width: (columnLabel.UPicMed1 || {}).ResizeWidth,
        Height: (columnLabel.UPicMed1 || {}).ResizeHeight,
      },
      MinImageSize: {
        Width: (columnLabel.UPicMed1 || {}).ColumnSize,
        Height: (columnLabel.UPicMed1 || {}).ColumnHeight,
      },
    }
    const ImprUsrId95List = AdmUsrImprReduxObj.ScreenDdlSelectors.ImprUsrId95(AdmUsrImprState);
    const ImprUsrId95 = currMst.ImprUsrId95;
    const IPicMed1 = currMst.IPicMed1 ? decodeEmbeddedFileObjectFromServer(currMst.IPicMed1) : null;
    const IPicMed1FileUploadOptions = {
      CancelFileButton: auxSystemLabels.CancelFileBtnLabel,
      DeleteFileButton: auxSystemLabels.DeleteFileBtnLabel,
      MaxImageSize: {
        Width: (columnLabel.IPicMed1 || {}).ResizeWidth,
        Height: (columnLabel.IPicMed1 || {}).ResizeHeight,
      },
      MinImageSize: {
        Width: (columnLabel.IPicMed1 || {}).ColumnSize,
        Height: (columnLabel.IPicMed1 || {}).ColumnHeight,
      },
    }
    const FailedAttempt1 = currMst.FailedAttempt1;
    const InputBy95List = AdmUsrImprReduxObj.ScreenDdlSelectors.InputBy95(AdmUsrImprState);
    const InputBy95 = currMst.InputBy95;
    const InputOn95 = currMst.InputOn95;
    const ModifiedBy95List = AdmUsrImprReduxObj.ScreenDdlSelectors.ModifiedBy95(AdmUsrImprState);
    const ModifiedBy95 = currMst.ModifiedBy95;
    const ModifiedOn95 = currMst.ModifiedOn95;
    const TestCulture95List = AdmUsrImprReduxObj.ScreenDdlSelectors.TestCulture95(AdmUsrImprState);
    const TestCulture95 = currMst.TestCulture95;
    const TestCurrency95 = currMst.TestCurrency95;
    const SignOff95 = currMst.SignOff95;

    const { dropdownMenuButtonList, bottomButtonList, hasDropdownMenuButton, hasBottomButton, hasRowButton } = this.state.Buttons;
    const hasActableButtons = hasBottomButton || hasRowButton || hasDropdownMenuButton;

    const isMobileView = this.state.isMobile;
    const useMobileView = (isMobileView && !(this.props.user || {}).desktopView);
    const fileFileUploadOptions = {
      CancelFileButton: 'Cancel',
      DeleteFileButton: 'Delete',
      MaxImageSize: {
        Width: 1024,
        Height: 768,
      },
      MinImageSize: {
        Width: 40,
        Height: 40,
      },
      maxSize: 5 * 1024 * 1024,
    }

    /* ReactRule: Master Render */

    /* ReactRule End: Master Render */

    return (
      <DocumentTitle title={siteTitle}>
        <div>
          <ModalDialog color={this.state.ModalColor} title={this.state.ModalTitle} onChange={this.OnModalReturn} ModalOpen={this.state.ModalOpen} message={this.state.ModalMsg} />
          <div className='account'>
            <div className='account__wrapper account-col'>
              <div className='account__card shadow-box rad-4'>
                {/* {!useMobileView && this.constructor.ShowSpinner(AdmUsrImprState) && <div className='panel__refresh'></div>} */}
                {useMobileView && <div className='tabs tabs--justify tabs--bordered-bottom'>
                  <div className='tabs__wrap'>
                    <NaviBar history={this.props.history} navi={naviBar} />
                  </div>
                </div>}
                <p className='project-title-mobile mb-10'>{siteTitle.substring(0, document.title.indexOf('-') - 1)}</p>
                <Formik
                  initialValues={{
                    cUsrImprId95: formatContent(UsrImprId95 || '', 'TextBox'),
                    cUsrId95: UsrId95List.filter(obj => { return obj.key === UsrId95 })[0],
                    cUPicMed1: UPicMed1,
                    cImprUsrId95: ImprUsrId95List.filter(obj => { return obj.key === ImprUsrId95 })[0],
                    cIPicMed1: IPicMed1,
                    cFailedAttempt1: formatContent(FailedAttempt1 || '', 'StarRating'),
                    cInputBy95: InputBy95List.filter(obj => { return obj.key === InputBy95 })[0],
                    cInputOn95: InputOn95 || new Date(),
                    cModifiedBy95: ModifiedBy95List.filter(obj => { return obj.key === ModifiedBy95 })[0],
                    cModifiedOn95: ModifiedOn95 || new Date(),
                    cTestCulture95: TestCulture95List.filter(obj => { return obj.key === TestCulture95 })[0],
                    cTestCurrency95: formatContent(TestCurrency95 || '', 'Currency'),
                    cSignOff95: formatContent(SignOff95 || '', 'Signature'),
                  }}
                  validate={this.ValidatePage}
                  onSubmit={this.SavePage}
                  key={currMst.key}
                  render={({
                    values,
                    errors,
                    touched,
                    isSubmitting,
                    dirty,
                    setFieldValue,
                    setFieldTouched,
                    handleChange,
                    submitForm
                  }) => (
                      <div>
                        {this.setDirtyFlag(dirty) &&
                          <Prompt
                            when={dirty}
                            message={auxSystemLabels.UnsavedPageMsg || ''}
                          />
                        }
                        <div className='account__head'>
                          <Row>
                            <Col xs={useMobileView ? 9 : 8}>
                              <h3 className='account__title'>{MasterRecTitle}</h3>
                              <h4 className='account__subhead subhead'>{MasterRecSubtitle}</h4>
                            </Col>
                            <Col xs={useMobileView ? 3 : 4}>
                              <ButtonToolbar className='f-right'>
                                {(this.constructor.ShowSpinner(AdmUsrImprState) && <Skeleton height='40px' />) ||
                                  <UncontrolledDropdown>
                                    <ButtonGroup className='btn-group--icons'>
                                      <i className={dirty ? 'fa fa-exclamation exclamation-icon' : ''}></i>
                                      {
                                        dropdownMenuButtonList.filter(v => !v.expose && !this.ActionSuppressed(authRow, v.buttonType, (currMst || {}).UsrImprId95)).length > 0 &&
                                        <DropdownToggle className='mw-50' outline>
                                          <i className='fa fa-ellipsis-h icon-holder'></i>
                                          {!useMobileView && <p className='action-menu-label'>{(screenButtons.More || {}).label}</p>}
                                        </DropdownToggle>
                                      }
                                    </ButtonGroup>
                                    {
                                      dropdownMenuButtonList.filter(v => !v.expose).length > 0 &&
                                      <DropdownMenu right className={`dropdown__menu dropdown-options`}>
                                        {
                                          dropdownMenuButtonList.filter(v => !v.expose).map(v => {
                                            if (this.ActionSuppressed(authRow, v.buttonType, (currMst || {}).UsrImprId95)) return null;
                                            return (
                                              <DropdownItem key={v.tid || v.order} onClick={this.ScreenButtonAction[v.buttonType]({ naviBar, submitForm, ScreenButton: v, mst: currMst, dtl: currDtl, useMobileView })} className={`${v.className}`}><i className={`${v.iconClassName} mr-10`}></i>{v.label}</DropdownItem>)
                                          })
                                        }
                                      </DropdownMenu>
                                    }
                                  </UncontrolledDropdown>
                                }
                              </ButtonToolbar>
                            </Col>
                          </Row>
                        </div>
                        <Form className='form'> {/* this line equals to <form className='form' onSubmit={handleSubmit} */}
                          {(selectedMst || {}).key ?
                            <div className='form__form-group'>
                              <div className='form__form-group-narrow'>
                                <div className='form__form-group-field'>
                                  <span className='radio-btn radio-btn--button btn--button-header h-20 no-pointer'>
                                    <span className='radio-btn__label color-blue fw-700 f-14'>{selectedMst.label || ''}</span>
                                    <span className='radio-btn__label__right color-blue fw-700 f-14'><span className='mr-5'>{selectedMst.labelR || ''}</span>
                                    </span>
                                  </span>
                                </div>
                              </div>
                              <div className='form__form-group-field'>
                                <span className='radio-btn radio-btn--button btn--button-header h-20 no-pointer'>
                                  <span className='radio-btn__label color-blue fw-700 f-14'>{selectedMst.detail || ''}</span>
                                  <span className='radio-btn__label__right color-blue fw-700 f-14'><span className='mr-5'>{selectedMst.detailR || ''}</span>
                                  </span>
                                </span>
                              </div>
                            </div>
                            :
                            <div className='form__form-group'>
                              <div className='form__form-group-narrow'>
                                <div className='form__form-group-field'>
                                  <span className='radio-btn radio-btn--button btn--button-header h-20 no-pointer'>
                                    <span className='radio-btn__label color-blue fw-700 f-14'>{NoMasterMsg}</span>
                                  </span>
                                </div>
                              </div>
                            </div>
                          }
                          <div className='w-100'>
                            <Row>
                              {(authCol.UsrImprId95 || {}).visible &&
                                <Col lg={6} xl={6}>
                                  <div className='form__form-group'>
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='20px' />) ||
                                      <label className='form__form-group-label'>{(columnLabel.UsrImprId95 || {}).ColumnHeader} {(columnLabel.UsrImprId95 || {}).ToolTip &&
                                        (<ControlledPopover id={(columnLabel.UsrImprId95 || {}).ColumnName} className='sticky-icon pt-0 lh-23' message={(columnLabel.UsrImprId95 || {}).ToolTip} />
                                        )}
                                      </label>
                                    }
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='36px' />) ||
                                      <div className='form__form-group-field'>
                                        <Field
                                          type='text'
                                          name='cUsrImprId95'
                                          disabled={(authCol.UsrImprId95 || {}).readonly ? 'disabled' : ''} />
                                      </div>
                                    }
                                    {errors.cUsrImprId95 && touched.cUsrImprId95 && <span className='form__form-group-error'>{errors.cUsrImprId95}</span>}
                                  </div>
                                </Col>
                              }
                              {(authCol.UsrId95 || {}).visible &&
                                <Col lg={6} xl={6}>
                                  <div className='form__form-group'>
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='20px' />) ||
                                      <label className='form__form-group-label'>{(columnLabel.UsrId95 || {}).ColumnHeader} <span className='text-danger'>*</span>{(columnLabel.UsrId95 || {}).ToolTip &&
                                        (<ControlledPopover id={(columnLabel.UsrId95 || {}).ColumnName} className='sticky-icon pt-0 lh-23' message={(columnLabel.UsrId95 || {}).ToolTip} />
                                        )}
                                      </label>
                                    }
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='36px' />) ||
                                      <div className='form__form-group-field'>
                                        <AutoCompleteField
                                          name='cUsrId95'
                                          onChange={this.FieldChange(setFieldValue, setFieldTouched, 'cUsrId95', false, values, [this.UsrId95Change])}
                                          onBlur={this.FieldChange(setFieldValue, setFieldTouched, 'cUsrId95', true)}
                                          onInputChange={this.UsrId95InputChange()}
                                          value={values.cUsrId95}
                                          defaultSelected={UsrId95List.filter(obj => { return obj.key === UsrId95 })}
                                          options={UsrId95List}
                                          filterBy={this.AutoCompleteFilterBy}
                                          disabled={(authCol.UsrId95 || {}).readonly ? true : false} />
                                      </div>
                                    }
                                    {errors.cUsrId95 && touched.cUsrId95 && <span className='form__form-group-error'>{errors.cUsrId95}</span>}
                                  </div>
                                </Col>
                              }

                              {(authCol.UPicMed1 || {}).visible &&
                                <Col lg={6} xl={6}>
                                  <div className='form__form-group'>
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='20px' />) ||
                                      <label className='form__form-group-label'>{(columnLabel.UPicMed1 || {}).ColumnHeader} {(columnLabel.UPicMed1 || {}).ToolTip &&
                                        (<ControlledPopover id={(columnLabel.UPicMed1 || {}).ColumnName} className='sticky-icon pt-0 lh-23' message={(columnLabel.UPicMed1 || {}).ToolTip} />
                                        )}
                                      </label>
                                    }
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='36px' />) ||
                                      <div className='form__form-group-field'>
                                        <Field
                                          component={FileInputField}
                                          name='cUPicMed1'
                                          options={{ ...fileFileUploadOptions, maxFileCount: 1 }}
                                          files={(this.BindFileObject(UPicMed1, values.cUPicMed1) || []).filter(f => !f.isEmptyFileObject)}
                                          label={(columnLabel.UPicMed1 || {}).ToolTip}
                                          disabled={true}
                                        />
                                      </div>
                                    }
                                    {errors.cUPicMed1 && touched.cUPicMed1 && <span className='form__form-group-error'>{errors.cUPicMed1}</span>}
                                  </div>
                                </Col>
                              }

                              {(authCol.ImprUsrId95 || {}).visible &&
                                <Col lg={6} xl={6}>
                                  <div className='form__form-group'>
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='20px' />) ||
                                      <label className='form__form-group-label'>{(columnLabel.ImprUsrId95 || {}).ColumnHeader} <span className='text-danger'>*</span>{(columnLabel.ImprUsrId95 || {}).ToolTip &&
                                        (<ControlledPopover id={(columnLabel.ImprUsrId95 || {}).ColumnName} className='sticky-icon pt-0 lh-23' message={(columnLabel.ImprUsrId95 || {}).ToolTip} />
                                        )}
                                      </label>
                                    }
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='36px' />) ||
                                      <div className='form__form-group-field'>
                                        <AutoCompleteField
                                          name='cImprUsrId95'
                                          onChange={this.FieldChange(setFieldValue, setFieldTouched, 'cImprUsrId95', false, values, [this.ImprUsrId95Change])}
                                          onBlur={this.FieldChange(setFieldValue, setFieldTouched, 'cImprUsrId95', true)}
                                          onInputChange={this.ImprUsrId95InputChange()}
                                          value={values.cImprUsrId95}
                                          defaultSelected={ImprUsrId95List.filter(obj => { return obj.key === ImprUsrId95 })}
                                          options={ImprUsrId95List}
                                          filterBy={this.AutoCompleteFilterBy}
                                          disabled={(authCol.ImprUsrId95 || {}).readonly ? true : false} />
                                      </div>
                                    }
                                    {errors.cImprUsrId95 && touched.cImprUsrId95 && <span className='form__form-group-error'>{errors.cImprUsrId95}</span>}
                                  </div>
                                </Col>
                              }

                              {(authCol.IPicMed1 || {}).visible &&
                                <Col lg={6} xl={6}>
                                  <div className='form__form-group'>
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='20px' />) ||
                                      <label className='form__form-group-label'>{(columnLabel.IPicMed1 || {}).ColumnHeader} {(columnLabel.IPicMed1 || {}).ToolTip &&
                                        (<ControlledPopover id={(columnLabel.IPicMed1 || {}).ColumnName} className='sticky-icon pt-0 lh-23' message={(columnLabel.IPicMed1 || {}).ToolTip} />
                                        )}
                                      </label>
                                    }
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='36px' />) ||
                                      <div className='form__form-group-field'>
                                        <Field
                                          component={FileInputField}
                                          name='cIPicMed1'
                                          options={{ ...fileFileUploadOptions, maxFileCount: 1 }}
                                          files={(this.BindFileObject(IPicMed1, values.cIPicMed1) || []).filter(f => !f.isEmptyFileObject)}
                                          label={(columnLabel.IPicMed1 || {}).ToolTip}
                                          disabled={true}
                                        />
                                      </div>
                                    }
                                    {errors.cIPicMed1 && touched.cIPicMed1 && <span className='form__form-group-error'>{errors.cIPicMed1}</span>}
                                  </div>
                                </Col>
                              }

                              {(authCol.FailedAttempt1 || {}).visible &&
                                <Col lg={6} xl={6}>
                                  <div className='form__form-group'>
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='20px' />) ||
                                      <label className='form__form-group-label'>{(columnLabel.FailedAttempt1 || {}).ColumnHeader} {(columnLabel.FailedAttempt1 || {}).ToolTip &&
                                        (<ControlledPopover id={(columnLabel.FailedAttempt1 || {}).ColumnName} className='sticky-icon pt-0 lh-23' message={(columnLabel.FailedAttempt1 || {}).ToolTip} />
                                        )}
                                      </label>
                                    }
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='36px' />) ||
                                      (<div className='form__form-group-field'>
                                        <Field
                                          type='text'
                                          name='cFailedAttempt1'
                                          disabled={(authCol.FailedAttempt1 || {}).readonly ? 'disabled' : ''} />
                                      </div>)
                                    }
                                    {errors.cFailedAttempt1 && touched.cFailedAttempt1 && <span className='form__form-group-error'>{errors.cFailedAttempt1}</span>}
                                  </div>
                                </Col>
                              }
                              {(authCol.InputBy95 || {}).visible &&
                                <Col lg={6} xl={6}>
                                  <div className='form__form-group'>
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='20px' />) ||
                                      <label className='form__form-group-label'>{(columnLabel.InputBy95 || {}).ColumnHeader} {(columnLabel.InputBy95 || {}).ToolTip &&
                                        (<ControlledPopover id={(columnLabel.InputBy95 || {}).ColumnName} className='sticky-icon pt-0 lh-23' message={(columnLabel.InputBy95 || {}).ToolTip} />
                                        )}
                                      </label>
                                    }
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='36px' />) ||
                                      <div className='form__form-group-field'>
                                        <DropdownField
                                          name='cInputBy95'
                                          onChange={this.DropdownChangeV1(setFieldValue, setFieldTouched, 'cInputBy95')}
                                          value={values.cInputBy95}
                                          options={InputBy95List}
                                          placeholder=''
                                          disabled={(authCol.InputBy95 || {}).readonly ? 'disabled' : ''} />
                                      </div>
                                    }
                                    {errors.cInputBy95 && touched.cInputBy95 && <span className='form__form-group-error'>{errors.cInputBy95}</span>}
                                  </div>
                                </Col>
                              }
                              {(authCol.InputOn95 || {}).visible &&
                                <Col lg={6} xl={6}>
                                  <div className='form__form-group'>
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='20px' />) ||
                                      <label className='form__form-group-label'>{(columnLabel.InputOn95 || {}).ColumnHeader} {(columnLabel.InputOn95 || {}).ToolTip &&
                                        (<ControlledPopover id={(columnLabel.InputOn95 || {}).ColumnName} className='sticky-icon pt-0 lh-23' message={(columnLabel.InputOn95 || {}).ToolTip} />
                                        )}
                                      </label>
                                    }
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='36px' />) ||
                                      <div className='form__form-group-field'>
                                        <DatePicker
                                          name='cInputOn95'
                                          onChange={this.DateChange(setFieldValue, setFieldTouched, 'cInputOn95', false)}
                                          onBlur={this.DateChange(setFieldValue, setFieldTouched, 'cInputOn95', true)}
                                          value={values.cInputOn95}
                                          selected={values.cInputOn95}
                                          disabled={(authCol.InputOn95 || {}).readonly ? true : false} />
                                      </div>
                                    }
                                    {errors.cInputOn95 && touched.cInputOn95 && <span className='form__form-group-error'>{errors.cInputOn95}</span>}
                                  </div>
                                </Col>
                              }
                              {(authCol.ModifiedBy95 || {}).visible &&
                                <Col lg={6} xl={6}>
                                  <div className='form__form-group'>
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='20px' />) ||
                                      <label className='form__form-group-label'>{(columnLabel.ModifiedBy95 || {}).ColumnHeader} {(columnLabel.ModifiedBy95 || {}).ToolTip &&
                                        (<ControlledPopover id={(columnLabel.ModifiedBy95 || {}).ColumnName} className='sticky-icon pt-0 lh-23' message={(columnLabel.ModifiedBy95 || {}).ToolTip} />
                                        )}
                                      </label>
                                    }
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='36px' />) ||
                                      <div className='form__form-group-field'>
                                        <DropdownField
                                          name='cModifiedBy95'
                                          onChange={this.DropdownChangeV1(setFieldValue, setFieldTouched, 'cModifiedBy95')}
                                          value={values.cModifiedBy95}
                                          options={ModifiedBy95List}
                                          placeholder=''
                                          disabled={(authCol.ModifiedBy95 || {}).readonly ? 'disabled' : ''} />
                                      </div>
                                    }
                                    {errors.cModifiedBy95 && touched.cModifiedBy95 && <span className='form__form-group-error'>{errors.cModifiedBy95}</span>}
                                  </div>
                                </Col>
                              }
                              {(authCol.ModifiedOn95 || {}).visible &&
                                <Col lg={6} xl={6}>
                                  <div className='form__form-group'>
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='20px' />) ||
                                      <label className='form__form-group-label'>{(columnLabel.ModifiedOn95 || {}).ColumnHeader} {(columnLabel.ModifiedOn95 || {}).ToolTip &&
                                        (<ControlledPopover id={(columnLabel.ModifiedOn95 || {}).ColumnName} className='sticky-icon pt-0 lh-23' message={(columnLabel.ModifiedOn95 || {}).ToolTip} />
                                        )}
                                      </label>
                                    }
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='36px' />) ||
                                      <div className='form__form-group-field'>
                                        <DatePicker
                                          name='cModifiedOn95'
                                          onChange={this.DateChange(setFieldValue, setFieldTouched, 'cModifiedOn95', false)}
                                          onBlur={this.DateChange(setFieldValue, setFieldTouched, 'cModifiedOn95', true)}
                                          value={values.cModifiedOn95}
                                          selected={values.cModifiedOn95}
                                          disabled={(authCol.ModifiedOn95 || {}).readonly ? true : false} />
                                      </div>
                                    }
                                    {errors.cModifiedOn95 && touched.cModifiedOn95 && <span className='form__form-group-error'>{errors.cModifiedOn95}</span>}
                                  </div>
                                </Col>
                              }
                              {(authCol.TestCulture95 || {}).visible &&
                                <Col lg={6} xl={6}>
                                  <div className='form__form-group'>
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='20px' />) ||
                                      <label className='form__form-group-label'>{(columnLabel.TestCulture95 || {}).ColumnHeader} {(columnLabel.TestCulture95 || {}).ToolTip &&
                                        (<ControlledPopover id={(columnLabel.TestCulture95 || {}).ColumnName} className='sticky-icon pt-0 lh-23' message={(columnLabel.TestCulture95 || {}).ToolTip} />
                                        )}
                                      </label>
                                    }
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='36px' />) ||
                                      <div className='form__form-group-field'>
                                        <AutoCompleteField
                                          name='cTestCulture95'
                                          onChange={this.FieldChange(setFieldValue, setFieldTouched, 'cTestCulture95', false, values, [this.TestCulture95Change])}
                                          onBlur={this.FieldChange(setFieldValue, setFieldTouched, 'cTestCulture95', true)}
                                          onInputChange={this.TestCulture95InputChange()}
                                          value={values.cTestCulture95}
                                          defaultSelected={TestCulture95List.filter(obj => { return obj.key === TestCulture95 })}
                                          options={TestCulture95List}
                                          filterBy={this.AutoCompleteFilterBy}
                                          disabled={(authCol.TestCulture95 || {}).readonly ? true : false} />
                                      </div>
                                    }
                                    {errors.cTestCulture95 && touched.cTestCulture95 && <span className='form__form-group-error'>{errors.cTestCulture95}</span>}
                                  </div>
                                </Col>
                              }
                              {(authCol.TestCurrency95 || {}).visible &&
                                <Col lg={6} xl={6}>
                                  <div className='form__form-group'>
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='20px' />) ||
                                      <label className='form__form-group-label'>{(columnLabel.TestCurrency95 || {}).ColumnHeader} {(columnLabel.TestCurrency95 || {}).ToolTip &&
                                        (<ControlledPopover id={(columnLabel.TestCurrency95 || {}).ColumnName} className='sticky-icon pt-0 lh-23' message={(columnLabel.TestCurrency95 || {}).ToolTip} />
                                        )}
                                      </label>
                                    }
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='36px' />) ||
                                      (<div className='form__form-group-field'>
                                        <Field
                                          type='text'
                                          name='cTestCurrency95'
                                          disabled={(authCol.TestCurrency95 || {}).readonly ? 'disabled' : ''} />
                                      </div>)
                                    }
                                    {errors.cTestCurrency95 && touched.cTestCurrency95 && <span className='form__form-group-error'>{errors.cTestCurrency95}</span>}
                                  </div>
                                </Col>
                              }
                              {(authCol.SignOff95 || {}).visible &&
                                <Col lg={6} xl={6}>
                                  <div className='form__form-group'>
                                    {((true && this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='20px' />) ||
                                      <label className='form__form-group-label'>{(columnLabel.SignOff95 || {}).ColumnHeader} {(columnLabel.SignOff95 || {}).ToolTip &&
                                        (<ControlledPopover id={(columnLabel.SignOff95 || {}).ColumnName} className='sticky-icon pt-0 lh-23' message={(columnLabel.SignOff95 || {}).ToolTip} />
                                        )}
                                      </label>
                                    }
                                    {((this.constructor.ShowSpinner(AdmUsrImprState)) && <Skeleton height='36px' />) ||
                                      <div className='form__form-group-field'>
                                        {values.cSignOff95 &&
                                          <SignaturePanel  
                                            name='cSignOff95' 
                                            src={values.cSignOff95} 
                                            onChange={this.SignatureChange(setFieldValue, setFieldTouched, 'cSignOff95')}
                                            value={values.cSignOff95} />
                                        }
                                      </div>
                                    }
                                    {errors.cSignOff95 && touched.cSignOff95 && <span className='form__form-group-error'>{errors.cSignOff95}</span>}
                                  </div>
                                </Col>
                              }
                            </Row>
                          </div>
                          <div className='form__form-group mart-5 mb-0'>
                            <Row className='btn-bottom-row'>
                              {useMobileView && <Col xs={3} sm={2} className='btn-bottom-column'>
                                <Button color='success' className='btn btn-outline-success account__btn' onClick={this.props.history.goBack} outline><i className='fa fa-long-arrow-left'></i></Button>
                              </Col>}
                              <Col
                                xs={useMobileView ? 9 : 12}
                                sm={useMobileView ? 10 : 12}>
                                <Row>
                                  {
                                    bottomButtonList
                                      .filter(v => v.expose)
                                      .map((v, i, a) => {
                                        if (this.ActionSuppressed(authRow, v.buttonType, (currMst || {}).UsrImprId95)) return null;
                                        const buttonCount = a.length - a.filter(x => this.ActionSuppressed(authRow, x.buttonType, (currMst || {}).UsrImprId95));
                                        const colWidth = parseInt(12 / buttonCount, 10);
                                        const lastBtn = i === (a.length - 1);
                                        const outlineProperty = lastBtn ? false : true;
                                        return (
                                          <Col key={v.tid || v.order} xs={colWidth} sm={colWidth} className='btn-bottom-column' >
                                            {(this.constructor.ShowSpinner(AdmUsrImprState) && <Skeleton height='43px' />) ||
                                              <Button color='success' type='button' outline={outlineProperty} className='account__btn' disabled={isSubmitting} onClick={this.ScreenButtonAction[v.buttonType]({ naviBar, submitForm, ScreenButton: v, mst: currMst, useMobileView })}>{v.label}</Button>
                                            }
                                          </Col>
                                        )
                                      })
                                  }
                                </Row>
                              </Col>
                            </Row>
                          </div>
                        </Form>
                      </div>
                    )}
                />
              </div>
            </div>
          </div>
        </div>
      </DocumentTitle>
    );
  };
};

const mapStateToProps = (state) => ({
  user: (state.auth || {}).user,
  error: state.error,
  AdmUsrImpr: state.AdmUsrImpr,
  global: state.global,
});

const mapDispatchToProps = (dispatch) => (
  bindActionCreators(Object.assign({},
    { LoadPage: AdmUsrImprReduxObj.LoadPage.bind(AdmUsrImprReduxObj) },
    { SavePage: AdmUsrImprReduxObj.SavePage.bind(AdmUsrImprReduxObj) },
    { DelMst: AdmUsrImprReduxObj.DelMst.bind(AdmUsrImprReduxObj) },
    { AddMst: AdmUsrImprReduxObj.AddMst.bind(AdmUsrImprReduxObj) },
    { SearchUsrId95: AdmUsrImprReduxObj.SearchActions.SearchUsrId95.bind(AdmUsrImprReduxObj) },
    { GetRefUsrId95: AdmUsrImprReduxObj.SearchActions.GetRefUsrId95.bind(AdmUsrImprReduxObj) },
    { SearchImprUsrId95: AdmUsrImprReduxObj.SearchActions.SearchImprUsrId95.bind(AdmUsrImprReduxObj) },
    { GetRefImprUsrId95: AdmUsrImprReduxObj.SearchActions.GetRefImprUsrId95.bind(AdmUsrImprReduxObj) },
    { SearchTestCulture95: AdmUsrImprReduxObj.SearchActions.SearchTestCulture95.bind(AdmUsrImprReduxObj) },
    { GetRefTestCulture95: AdmUsrImprReduxObj.SearchActions.GetRefTestCulture95.bind(AdmUsrImprReduxObj) },
    { showNotification: showNotification },
    { setTitle: setTitle },
    { setSpinner: setSpinner },
  ), dispatch)
)

export default connect(mapStateToProps, mapDispatchToProps)(MstRecord);
