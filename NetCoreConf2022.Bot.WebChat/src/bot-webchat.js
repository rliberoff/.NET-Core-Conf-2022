import { idWebChat, idWebChatButton, idWebChatHeader, renderPrincipalTagIndex, renderScriptIndex } from './bot-webchat-html.js'
import { generateUUID } from './utils.js'

renderScriptIndex();

let initiated = false;

async function initChat() {

    if (!initiated) {
        const styleOptions = {
            bubbleBorderRadius: '10px 10px 10px 0px',
            bubbleFromUserBorderRadius: '10px 10px 0px 10px',
            bubbleBackground: '#F3F3F3',
            bubbleFromUserBackground: '#CEBDDB',
            botAvatarInitials: process.env.botAvatarInitials,
            botAvatarImage: process.env.botAvatarImage,
            botAvatarBackgroundColor: 'transparent',
            userAvatarInitials: process.env.userAvatarInitials,
            userAvatarImage: process.env.userAvatarImage,
            userAvatarBackgroundColor: '#662D91',
            hideUploadButton: true,
        };

        const userId = generateUUID();
        const userName = process.env.username;
        const userLocale = navigator.locale;

        const store = initStore();

        const attachmentMiddleware = initAttachmentMiddleware(store);

        window.WebChat.renderWebChat(
            {
                directLine: window.WebChat.createDirectLine({ token: process.env.dlToken }),
                userID: userId,
                username: userName,
                locale: userLocale,
                store: store,
                styleOptions: styleOptions,
                attachmentMiddleware: attachmentMiddleware,
            },
            document.getElementById(idWebChat)
        );

        document.querySelector(`#${idWebChat} > *`).focus();

        showChat();

        initiated = true;
    } else {
        showChat();
    }
}

function initAttachmentMiddleware(store) {
    return () =>
        (next) =>
            ({ activity, attachment, ...others }) => {
                const { activities } = store.getState();
                const messageActivities = activities.filter((act) => act.type === 'message');
                const recentBotMessage = messageActivities.pop() === activity;

                if (
                    attachment.contentType === 'application/vnd.microsoft.card.adaptive'
                ) {
                    return Components.AdaptiveCardContent({
                        actionPerformedClassName: 'card__action--performed',
                        content: attachment.content,
                        disabled: !recentBotMessage,
                    });
                }
                else {
                    return next({ activity, attachment, ...others });
                }
            };
}

function initStore() {
    return window.WebChat.createStore({}, ({ dispatch }) => next => action => {
        // From https://codez.deedx.cz/posts/web-chat-events-summary/
        if (action.type === 'DIRECT_LINE/POST_ACTIVITY') {
            const jsonLocalStorageToChannelData = getAllItemLocalStorageParseChannelData(process.env.localStoragePrefix);
            action = window.simpleUpdateIn(
                action,
                ['payload', 'activity', 'channelData'],
                () => jsonLocalStorageToChannelData
            );
        }

        if (action.type === 'DIRECT_LINE/CONNECT_FULFILLED') {
            const jsonLocalStorageToChannelData = getAllItemLocalStorageParseChannelData(process.env.localStoragePrefix);

            dispatch({
                type: 'WEB_CHAT/SEND_EVENT',
                payload: {
                    name: 'webchat/greetings',
                    value: jsonLocalStorageToChannelData
                },
            });
        }

        return next(action);
    });
}

function hideChat() {
    document.getElementById(idWebChatButton).style.display = 'block';
    document.getElementById(idWebChat).style.display = 'none';
    document.getElementById(idWebChatHeader).style.display = 'none';
}

function showChat() {
    document.getElementById(idWebChatButton).style.display = 'none';
    document.getElementById(idWebChat).style.display = 'block';
    document.getElementById(idWebChatHeader).style.display = 'block';
}

function getAllItemLocalStorageParseChannelData(prefixKey) {
    const jsonTemp = {};

    if (typeof prefixKey !== 'undefined') {
        const keys = Object.keys(localStorage);
        let i = 0, key;

        for (; key = keys[i]; i++) {
            if (key.includes(prefixKey)) {
                jsonTemp[key] = localStorage.getItem(key);
            }
        }
    }

    return jsonTemp;
}

(function () {
    renderPrincipalTagIndex();
})();

window.initChat = initChat;
window.hideChat = hideChat;
