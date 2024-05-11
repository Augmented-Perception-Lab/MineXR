import { initializeApp } from "firebase/app";
import { getAnalytics } from "firebase/analytics";

import { getFirestore } from "firebase/firestore";
import { getStorage } from "firebase/storage";

const firebaseConfig = {
  apiKey: "xxxxx",
  authDomain: "xxxxxx.firebaseapp.com",
  databaseURL: "https://xxxxxx-default-rtdb.firebaseio.com",
  projectId: "xxxxxx",
  storageBucket: "xxxxxx.appspot.com",
  messagingSenderId: "xxxxxxxxxx",
  appId: "xxxxxxxx",
  measurementId: "xxxxxxxx"
};

const app = initializeApp(firebaseConfig);
const firestore = getFirestore(app);
const storage = getStorage(app);

export {firestore, storage};
