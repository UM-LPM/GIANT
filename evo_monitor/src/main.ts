import './assets/main.css';

import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import Toast from 'vue-toastification';
import 'vue-toastification/dist/index.css';
import { createPinia } from 'pinia';

import { Quasar, Dark } from 'quasar';

// Import icon libraries
import '@quasar/extras/material-icons/material-icons.css';
import '@quasar/extras/fontawesome-v6/fontawesome-v6.css';
import '@quasar/extras/bootstrap-icons/bootstrap-icons.css';

// Import Quasar css
import 'quasar/src/css/index.sass';

const app = createApp(App)

app.use(router)
app.use(Toast);
app.use(createPinia())

app.use(Quasar, {
    plugins: {}, // import Quasar plugins and add here
})

Dark.set(false);

app.mount('#app')