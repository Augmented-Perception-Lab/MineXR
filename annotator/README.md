# MineXR - Web Annotator

## Run
1. Set up a new Google Firebase project in Google Firebase Console: [https://console.firebase.google.com/](https://console.firebase.google.com/).
2. Configure a Realtime Database and Cloud Storage for the project.
3. Replace the variable `firebaseConfig` in `firebase.js` with the firease configuration for your project.
4. Update `baseStorageUrl` in `FuncComponent.js` to the base storage URL of the Firebase Cloud Storage.
5. Run `npm start` to open the annotator in your browser. 


## Using the annotator
1. Load data.
- For the first time, click `Import all data`. This imports all widget data from the connected Google Firebase project. Certain data can be excluded by specifying participant IDs in `handleLoadData.js`. Once data is imported, the main interface shows one widget at a time in an ascending order of the participant name and creation time. 
- For the next loads (to load newly collected data), clicek `Update new data`. This imports all new widget data from Google Firebase excluding those in the exclusion list. 
2. Create a new annotation.
- Fill in all annotation fields. The text fields and dropdown field have an auto-completion feature, where it shows previous anntotations with the matching word(s).
- Click the `Submit` button.
3. Modify an annotation.
- Locate the annotation you want to modify in the list below the annotation.
- Use Search, Filter, or Sort feature to facilitate the annotation search.
- Click the annotation you want to modify. It will load that annotation on the interface above.
- Change the field you want to modify.
- Click the `Submit` button.
4. Export annotations
- Click the `Export all annotations` button, which saves all annotations made so far into a .json file.

## Keyboard shortcuts
These keyboard shortcuts facilitate the annotation process:
- `tab` : Use tab to move to the next annotation field
- `space` :  
  * Toggle checkboxes (Cropped?, Blurred?, Placeholder?, and UI Component Type(s))
  * Expand dropdown (Category)
- `arrow keys` : Up, down to move selection in dropdown (auto-completion or Category)
- `enter` :
  * Select from the dropdown list (auto-completion or Category).
  * Submit
