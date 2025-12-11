import './assets/main.css'
import {createApp} from 'vue';
import {createPinia} from 'pinia';
import App from './App.vue';
import router from './router'
import GlobalLoading from './views/GlobalLoadingView.vue';
// Import API client to initialize interceptors
import '@/services/api/client';
import { preloadTopicPhotos } from '@/utils/imagePreloader';

const pinia = createPinia();

const app = createApp(App).use(pinia).use(router);

app.component('global-loading', GlobalLoading);

app.mount('#app');

// Preload topic card photos after first paint using requestIdleCallback
if ('requestIdleCallback' in window) {
  requestIdleCallback(() => {
    preloadTopicPhotos();
  });
} else {
  // Fallback for browsers without requestIdleCallback (Safari)
  setTimeout(() => {
    preloadTopicPhotos();
  }, 1);
}
