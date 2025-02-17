// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
//self.addEventListener('fetch', () => { });
///**  
//     * Register Service Worker  
//     */
//navigator.serviceWorker
//    .register('version.js', { scope: '/' })
//    .then(() => {
//        console.log('Service Worker Registered');
//    });
