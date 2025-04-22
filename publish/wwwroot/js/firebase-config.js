// Your web app's Firebase configuration
const firebaseConfig = {
    apiKey: "AIzaSyADwqESFUqSaL8i0wIzytnjoVTdwtaWwcw",
  authDomain: "geetajiaspdotnet.firebaseapp.com",
  projectId: "geetajiaspdotnet",
  storageBucket: "geetajiaspdotnet.firebasestorage.app",
  messagingSenderId: "855027140433",
  appId: "1:855027140433:web:3e80c62ad0161b942c3570",
  measurementId: "G-ETBG4FVWR8"
};

// Initialize Firebase
firebase.initializeApp(firebaseConfig);
const db = firebase.firestore(); 

// Import the functions you need from the SDKs you need
// import { initializeApp } from "firebase/app";
// import { getAnalytics } from "firebase/analytics";
// TODO: Add SDKs for Firebase products that you want to use
// https://firebase.google.com/docs/web/setup#available-libraries

// Your web app's Firebase configuration
// For Firebase JS SDK v7.20.0 and later, measurementId is optional
// const firebaseConfig = {
//   apiKey: "AIzaSyADwqESFUqSaL8i0wIzytnjoVTdwtaWwcw",
//   authDomain: "geetajiaspdotnet.firebaseapp.com",
//   projectId: "geetajiaspdotnet",
//   storageBucket: "geetajiaspdotnet.firebasestorage.app",
//   messagingSenderId: "855027140433",
//   appId: "1:855027140433:web:3e80c62ad0161b942c3570",
//   measurementId: "G-ETBG4FVWR8"
// };

// // Initialize Firebase
// const app = initializeApp(firebaseConfig);
// const analytics = getAnalytics(app);



// npm install -g firebase-tools