// import { useContext, useEffect } from 'react';
// import { UNSAFE_NavigationContext } from 'react-router-dom';

// /**
//  * Block navigation if `when` is true and user doesn't confirm.
//  */
// export function usePrompt(when: boolean, message: string) {
//   const context = useContext(UNSAFE_NavigationContext); // ✅ always called
//   const navigator = context?.navigator;

//   useEffect(() => {
//     if (typeof window === 'undefined') return;
//     if (!when || !navigator) return;

//     const originalPush = navigator.push;
//     const originalReplace = navigator.replace;

//     navigator.push = (...args: any[]) => {
//       if (window.confirm(message)) {
//         originalPush(...args);
//       }
//     };

//     navigator.replace = (...args: any[]) => {
//       if (window.confirm(message)) {
//         originalReplace(...args);
//       }
//     };

//     return () => {
//       navigator.push = originalPush;
//       navigator.replace = originalReplace;
//     };
//   }, [when, message, navigator]);
// }
