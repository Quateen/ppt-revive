import { createRoot } from 'react-dom/client';
import App from './App.tsx';
import './index.css';

import { Provider } from 'react-redux';
import store from './app-redux/store.ts';

import { GoogleOAuthProvider } from '@react-oauth/google';
import { AppConfig } from './config/index.ts';

createRoot(document.getElementById('root')!).render(
  <GoogleOAuthProvider clientId={AppConfig.GOOGLE_OAUTH_CLIENT_ID}>
    <Provider store={store}>
      <App />
    </Provider>
  </GoogleOAuthProvider>
);
