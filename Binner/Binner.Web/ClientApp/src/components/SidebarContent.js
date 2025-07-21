import { useCallback, useEffect, useState } from "react";
import PropTypes from "prop-types";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { Link } from "react-router-dom";
import { Icon, Grid, Button, Popup } from "semantic-ui-react";
import { ThemeChangeToggle } from "./ThemeChangeToggle";
import { isAdmin } from "../common/authentication";
import { config } from "../common/config";
import { fetchApi } from "../common/fetchApi";
import customEvents from '../common/customEvents';
import { getLocalData, setLocalData } from "../common/storage";
import { getFriendlyElapsedTime, getTimeDifference, getDifferenceInMinutes } from "../common/datetime";
import { ReleaseNotesModal } from "./modals/ReleaseNotesModal";
import { parseJSON } from "date-fns";
import _ from "underscore";
import Logo from "../layouts/logo-light.svg?react";

export function SidebarContent({ openSidebar, version, latestVersion, latestVersionUrl, ...rest }) {
  const MAX_MESSAGE_LEN = 165;

  const getViewPreference = (preferenceName) => {
    return getLocalData(preferenceName, { settingsName: 'sidebar' })
  };

  const setViewPreference = (preferenceName, value, options) => {
    setLocalData(preferenceName, value, { settingsName: 'sidebar', ...options });
  };

  const { t } = useTranslation('en');
  let navigate = useNavigate();
  const [isViewMessageModalOpen, setIsViewMessageModalOpen] = useState(false);
  const [viewMessage, setViewMessage] = useState(null);
  const [viewMessageTitle, setViewMessageTitle] = useState(null);
  const [viewMessageDescription, setViewMessageDescription] = useState(null);
  const [systemMessages, setSystemMessages] = useState([]);

  const handleOpenSidebar = () => {
    if(openSidebar) {
      openSidebar();
      setViewPreference('lastOpenTime', new Date());
    }
  };

  /**
     * When we receive system messages, update it.
     */
  const updateSystemMessages = useCallback((systemMessagesData) => {
    setSystemMessages(systemMessagesData);

    if (_.find(systemMessagesData, x => x.readDateUtc === null)) {
      // open sidebar, but lets track how often it opens so we don't annoy the user
      const MaxAutoOpenMinutes = 15; // only open if more than 15 min has elapsed
      const lastOpenTime = getViewPreference('lastOpenTime');
      if (!lastOpenTime || getDifferenceInMinutes(parseJSON(lastOpenTime)) > MaxAutoOpenMinutes)
        handleOpenSidebar();

      // mark messages as read
      setTimeout(async () => {
        const unreadMessageIds = _.filter(systemMessagesData, i => i.readDateUtc === null).map(x => x.messageId);
        if (unreadMessageIds.length > 0) {
          await fetchApi("/api/system/messages/read", {
            method: "PUT",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({
              messageIds: unreadMessageIds
            }),
          });

          // mark messages as read in UI after some time
          setTimeout(() => {
            setSystemMessages(systemMessagesData.map(x => { x.readDateUtc = new Date(); return x; }));
            setTimeout(() => {
              setSystemMessages(systemMessagesData.map(x => { x.readDateUtc = ''; return x; }));
            }, 5000);
          }, 2000);
        }
      }, 2000);
    }
  }, []);

  useEffect(() => {
    const systemMessagesEvent = customEvents.subscribe("systemMessages", (data) => updateSystemMessages(data));
    return () => {
      customEvents.unsubscribe(systemMessagesEvent);
    };
  }, [updateSystemMessages]);

  const handleReadMessage = (e, control, msg) => {
    setIsViewMessageModalOpen(true);
    setViewMessage(msg.message);
    setViewMessageTitle(msg.title);
    setViewMessageDescription(msg?.dateCreatedUtc !== null ? getFriendlyElapsedTime(getTimeDifference(new Date(), parseJSON(msg.dateCreatedUtc)), true) : '');
  };

  const handleViewVersion = (e) => {
    e.preventDefault();
    e.stopPropagation();
    window.open(latestVersionUrl, "_blank");
  };

  const getReadIcon = (readDate) => {
    if (readDate === null) return <Icon className="message-read" name="circle" color="blue" size="mini" style={{ verticalAlign: 'middle' }} />;
    else if (readDate instanceof Date) return <Icon className="message-read" name="circle" color="grey" size="mini" style={{ verticalAlign: 'middle' }} />;
    return "";
  };

  const route = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    navigate(url);
  };

  const handleRefreshSystemMessages = async () => {
    // fetch system messages
    const messages = await fetchApi("/api/system/messages");
    if (messages.responseObject.ok) {
      customEvents.notifySubscribers("systemMessages", messages.data);
    }
  };

  const handleViewMessageModalClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setIsViewMessageModalOpen(false);
  };

  return (<div className="content-container">
    <ReleaseNotesModal
      header={t('comp.sidebar.systemMessages', "System Messages")}
      description={<div><h1>{viewMessageTitle}</h1><span className="datetime relative">{viewMessageDescription}</span></div>}
      text={viewMessage}
      isOpen={isViewMessageModalOpen || false}
      onClose={handleViewMessageModalClose}
    />
    <div className="content-header">
      <div className="top-controls"><ThemeChangeToggle dark /></div>
      <h1><Logo width="18" height="18" /> <span>Binner</span></h1>
      <header>
        <div className="version">{version ? <span>{t('footer.version', "Version")}: {version} ({latestVersion?.replace('v', '') === version || config.BINNERIO === "true" ? 'latest' : <Link onClick={handleViewVersion}>{t('label.updateTo', "update to")} {latestVersion}</Link>})</span> : ""}</div>
      </header>
    </div>
    { /** TOP */}
    <div className="content-top">
      <div className="messages">
        <h2>
          <Popup
            content={<p>{t('label.refresh', "Refresh")}</p>}
            trigger={<Link size="medium" onClick={handleRefreshSystemMessages} style={{ marginLeft: '5px' }}>
              <Icon name="refresh" />
            </Link>}
          />
          {t('comp.sidebar.systemMessages', "System Messages")}</h2>
        <ul className="messagelist">
          {systemMessages.length > 0
            ? systemMessages.map((msg, key) => (
              <li key={key} className="message">
                <div>
                  <header>{getReadIcon(msg.readDateUtc)}{msg.title}</header>
                  <span className="date">{msg?.dateCreatedUtc !== null ? getFriendlyElapsedTime(getTimeDifference(new Date(), parseJSON(msg.dateCreatedUtc)), true) : ''}</span>

                  {msg.message.length > MAX_MESSAGE_LEN ? (<><p>{msg.message.substring(0, MAX_MESSAGE_LEN)}...</p><Link className="small" onClick={(e, control) => handleReadMessage(e, control, msg)}>Read more...</Link></>) : <p>{msg.message}</p>}
                </div>
              </li>
            ))
            : <li><div>{t('comp.sidebar.noMessages', "No messages available")}</div></li>}
        </ul>
      </div>
    </div>
    { /** BOTTOM */}
    <div className="content-bottom">
      <div className="quicklinks centered">
        <h3>{t('comp.sidebar.quickLinks', "Quick Links")}</h3>
        <Grid columns={5} centered>
          <Grid.Row textAlign="center">
            <Grid.Column textAlign="center"><Popup content={<p>{t('page.home.searchInventory', "Search Inventory")}</p>} trigger={<div className="shadow"><Button basic text={t('page.home.searchInventory', "Search Inventory")} icon="search" color="blue" onClick={e => route(e, '/inventory')} /></div>} /></Grid.Column>
            <Grid.Column textAlign="center"><Popup content={<p>{t('page.home.tools', "Tools")}</p>} trigger={<div className="shadow"><Button basic text={t('page.home.tools', "Tools")} icon="wrench" color="green" onClick={e => route(e, '/tools')} /></div>} /></Grid.Column>
            <Grid.Column textAlign="center"><Popup content={<p>{t('page.home.bom', "BOM")}</p>} trigger={<div className="shadow"><Button basic text={t('page.home.bom', "BOM")} icon="list alternate outline" color="orange" onClick={e => route(e, '/bom')} /></div>} /></Grid.Column>
            <Grid.Column textAlign="center"><Popup content={<p>{t('page.home.settings', "Settings")}</p>} trigger={<div className="shadow"><Button basic text={t('page.home.settings', "Settings")} icon="cog" color="violet" onClick={e => route(e, '/settings')} /></div>} /></Grid.Column>
            {isAdmin() ? <Grid.Column textAlign="center"><Popup content={<p>{t('page.home.admin', "Admin")}</p>} trigger={<div className="shadow"><Button basic text={t('page.home.admin', "Admin")} icon="cog" color="red" onClick={e => route(e, '/admin')} /></div>} /></Grid.Column> : ""}
          </Grid.Row>
        </Grid>
      </div>
    </div>
  </div>);
}

SidebarContent.propTypes = {
  version: PropTypes.string,
  latestVersion: PropTypes.string,
  latestVersionUrl: PropTypes.string,
  openSidebar: PropTypes.func
};