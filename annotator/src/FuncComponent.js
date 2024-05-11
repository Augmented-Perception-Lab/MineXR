import {useState, useEffect} from "react";
import {
    Box, Button, CheckBox, CheckBoxGroup, Form, FormField, Grid, Image as GrommetImage,
    Paragraph, Select, Text, TextArea, TextInput
} from "grommet";
import {Refresh} from "grommet-icons";
import {categories} from "./settings";

// function UrlExists(url) {
//
//     let img = new Image();
//     img.src = url;
//     console.log(img.height !== 0);
//     return img.height !== 0;
//
// }

function UrlExists(imageUrl) {
    return new Promise((resolve, reject) => {
        let imageData = new Image();

        imageData.onload = () => {
            resolve();
        }

        imageData.onerror = () => {
            reject()
        }

        imageData.src = imageUrl;
    })
}

const updateImageStoragePath = (imagePath, setImagePath) => {
    let baseStorageUrl = "https://firebasestorage.googleapis.com/v0/b/xxxxxx.appspot.com/o/";
    baseStorageUrl += imagePath.replace(/\//gi, "%2F");
    baseStorageUrl += "?alt=media";

    UrlExists(baseStorageUrl)
      .then(function(result) {
          setImagePath(baseStorageUrl);
      }, function(error) {
          baseStorageUrl = baseStorageUrl.replace(".png", ".jpeg");
          console.log("replaced url: ", baseStorageUrl);
          setImagePath(baseStorageUrl);
      });
    // if (!UrlExists(baseStorageUrl)) {
    //     baseStorageUrl = baseStorageUrl.replace(".png", ".jpeg");
    //     console.log("replaced url: ", baseStorageUrl);
    // }
}

const getImageStoragePath = (imagePath) => {
    let baseStorageUrl = "https://firebasestorage.googleapis.com/v0/b/xxxxxx.appspot.com/o/";
    baseStorageUrl += imagePath.replace(/\//gi, "%2F");
    baseStorageUrl += "?alt=media";
    return baseStorageUrl;
}

const FuncComponent = ({func, annotation, addAnnotation,
                           appNames, screenshotDescs, widgetDescs, excludedParts, forceNext}) => {

    /* State values for attributes */
    const [appNameValue, setAppNameValue] = useState(annotation == null ? "" : annotation.appName);
    const [screenDescValue, setScreenDescValue] = useState(annotation == null ? "" : annotation.screenDesc);
    const [widgetDescValue, setWidgetDescValue] = useState(annotation == null ? "" : annotation.widgetDesc);
    const [excludedPartsValue, setExcludedPartsValue] = useState(annotation == null ? "" : annotation.excludedParts);
    const [isCropped, setIsCropped] = useState(annotation == null ? true : annotation.isCropped);
    const [isBlurred, setIsBlurred] = useState(annotation == null ? false : annotation.isBlurred);
    const [isPlaceholder, setIsPlaceholder] = useState(annotation == null ? false : annotation.isPlaceholder);
    const [category, setCategory] = useState(annotation == null ? "" : annotation.category);
    const [uiTypes, setUiTypes] = useState(annotation == null ? [] : annotation.uiTypes);
    const [notes, setNotes] = useState(annotation == null ? "" : annotation.notes);

    const [value, setValue] = useState(annotation == null ? {anchorId: '', appName: '', screenDesc: '', widgetDesc: '', excludedParts: '', isCropped: true,
        isBlurred: false, isPlaceholder: false, appCategory: '', uiTypes: [], notes: ''} : annotation);

    const [newScreenImagePath, setNewScreenImagePath] = useState(func.screenshotImagePath);
    const [renderNewImage, setRenderNewImage] = useState(false);
    // const [widgetImagePath, setWidgetImagePath] = useState(func.widgetImagePath);

    const resetValues = () => {
        setAppNameValue("");
        setScreenDescValue("");
        setWidgetDescValue("");
        setExcludedPartsValue("");
        setIsCropped(true);
        setIsBlurred(false);
        setIsPlaceholder(false);
        setCategory("");
        setUiTypes([]);
        setNotes("");
    }
    // useEffect(() => {
    //     if (annotation != null) {
    //         setAppNameValue(annotation.appName);
    //         setScreenDescValue(annotation.screenDesc);
    //         setWidgetDescValue(annotation.widgetDesc);
    //         setExcludedPartsValue(annotation.excludedParts);
    //         setIsCropped(annotation.isCropped);
    //         setIsBlurred(annotation.isBlurred);
    //         setIsPlaceholder(annotation.isPlaceholder);
    //         setCategory(annotation.category);
    //         setUiTypes(annotation.uiTypes);
    //         setNotes(annotation.notes);
    //     }
    // }, []);

    return <Grid alignSelf="center"
                 columns={['medium', 'medium', 'medium']}
                 rows={['xsmall', 'large', 'small']}
                 gap="medium"
                 areas={[
                     { name: 'info', start: [0, 0], end: [2, 0]},
                     { name: 'screenshot', start: [0, 1], end: [0, 1] },
                     { name: 'crop', start: [1, 1], end: [1, 1] },
                     { name: 'annotations', start: [2, 1], end: [2, 2] },
                     { name: 'descriptions', start: [0, 2], end: [1, 2] }
                 ]}
    >
        <Box gridArea="info">
            <Text><Text weight="bold" color="brand">Anchor ID:</Text> {func.anchorId}</Text>
            <Text><Text weight="bold" color="brand">Participant:</Text> {func.pId}</Text>
            <Text><Text weight="bold" color="brand">Environment:</Text> {func.envId}</Text>
            <Text><Text weight="bold" color="brand">Task:</Text> {func.taskId}</Text>
        </Box>
        <Box gridArea="screenshot" direction="column" margin="small">
            <Box direction="row" alignSelf="center">
                <Text alignSelf="center" margin="small" color="brand" weight="bold">Screenshot</Text>
                <Button secondary onClick={() => {
                    updateImageStoragePath(func.screenshotImagePath, setNewScreenImagePath);
                    setRenderNewImage(true)}}
                ><Refresh color="focus"/></Button>
            </Box>
            {renderNewImage
              ? <GrommetImage fit="contain" src={newScreenImagePath} />
                : <GrommetImage fit="contain" src={getImageStoragePath(func.screenshotImagePath)} />}

        </Box>
        <Box gridArea="crop" direction="column" margin="small">
            <Text alignSelf="center" margin="small" color="brand" weight="bold">Crop</Text>
            {/*<img key={value.anchorId}*/}
            {/*     src={widgetImagePath}/>*/}
            <GrommetImage
                fit="contain"
                // key={widgetImagePath}
                src={getImageStoragePath(func.widgetImagePath)}
            />
        </Box>
        <Box gridArea="descriptions">
            <Text><Text color="brand" weight="bold">Input controls:</Text> checkboxes, radio buttons, dropdown lists, list boxes, buttons, toggles, text fields, date field</Text>
            <Text><Text color="brand" weight="bold">Navigation components:</Text> breadcrumb, slider, search field, pagination, slider, tags, icons</Text>
            <Text><Text color="brand" weight="bold">Informational Components:</Text> tooltips, icons, progress bar, notifications, message boxes, modal windows</Text>
            <Text><Text color="brand" weight="bold">Containers:</Text> div, accordion</Text>
        </Box>
        {/* <Box>{func.widgetImagePath}</Box> */}
        <Box gridArea="annotations">
            <Form
                value={value}
                onChange={nextValue => setValue(nextValue)}
                onReset={() => setValue({})}
                onSubmit={({ value }) => {
                    console.log(value);
                    addAnnotation(value, func);
                    setValue( {anchorId: "", appName: "", screenDesc: "", widgetDesc: "", excludedParts: "",
                        isCropped: true, isBlurred: false, isPlaceholder: false, category: "", uiTypes: [], notes: ""
                    });
                    // updateImageStoragePath(func.widgetImagePath, setWidgetImagePath);
                    // updateImageStoragePath(func.screenshotImagePath, setScreenImagePath);
                    resetValues();
                    setRenderNewImage(false);
                }}>
                <FormField label="Application name" htmlFor="appName" name="appName">
                    <TextInput id="appName" placeholder="type here" name="appName"
                               onChange={event => setAppNameValue(event.target.value)}
                               suggestions={appNames.filter(sug => ((sug !== null) && (sug.includes(appNameValue))))}/>
                </FormField>
                <FormField label="Screenshot description" htmlFor="screenDesc" name="screenDesc">
                    <TextInput id="screenDesc" placeholder="type here" name="screenDesc"
                               onChange={event => setScreenDescValue(event.target.value)}
                               suggestions={screenshotDescs.filter(sug => ((sug !== null) && (sug.includes(screenDescValue))))}/>
                </FormField>
                <FormField label="Widget description" htmlFor="widgetDesc" name="widgetDesc">
                    <TextInput id="widgetDesc" placeholder="type here" name="widgetDesc"
                               onChange={event => setWidgetDescValue(event.target.value)}
                               suggestions={widgetDescs.filter(sug => ((sug !== null) && (sug.includes(widgetDescValue))))}/>
                </FormField>
                <FormField label="Excluded parts" htmlFor="excludedParts" name="excludedParts">
                    <TextInput id="excludedParts" placeholder="type here" name="excludedParts"
                               onChange={event => setExcludedPartsValue(event.target.value)}
                               suggestions={excludedParts.filter(sug => ((sug !== null) && (sug.includes(excludedPartsValue))))}/>
                </FormField>
                <FormField htmlFor="isCropped" name="isCropped">
                    <CheckBox checked={isCropped} label={"Cropped?"} name="isCropped" reverse={true}
                              onChange={(event) => setIsCropped(event.target.checked)} />
                </FormField>
                <FormField htmlFor="isBlurred" name="isBlurred">
                    <CheckBox checked={isBlurred} label={"Blurred?"} name="isBlurred" reverse={true}
                              onChange={(event) => setIsBlurred(event.target.checked)} />
                </FormField>
                <FormField htmlFor="isPlaceholder" name="isPlaceholder">
                    <CheckBox checked={isPlaceholder} label={"Placeholder?"} name="isPlaceholder" reverse={true}
                              onChange={(event) => setIsPlaceholder(event.target.checked)} />
                </FormField>
                <FormField label="Category" name="appCategory" htmlFor="appCategory">
                    <Select name="appCategory"
                        options={categories} value={category} onChange={({ option }) => setCategory(option)} />
                </FormField>
                <FormField label="UI Component Type(s)" htmlFor="uiTypes" name="uiTypes">
                    <CheckBoxGroup
                        options={["Input Controls", "Navigational Components", "Informational Components", "Containers"]}
                        name="uiTypes"
                        value={uiTypes}
                        onChange={(event) => setUiTypes(event.value)}
                        // value={uiTypes}
                        // onChange={({uiTypeValue, option}) => {
                        //     console.log("UI types", value)
                        //     console.log("Option:", option)
                        //     setUiTypes(uiTypeValue)
                        // }}
                    />
                </FormField>

                <FormField label="Notes" name="notes" htmlFor="notes">
                    <TextArea
                        name="notes"
                        placeholder="type here"
                        value={notes}
                        onChange={event => setNotes(event.target.value)} />
                </FormField>
                {/*  Add Notes  */}
                {/* <FormField label="Category" name="category">
          <TextInput placeholder="type here" />
        </FormField>
        <FormField label="UI element type" name="ui_element_type">
          <TextInput placeholder="type here" />
        </FormField> */}

                {/* <FormField label="Grouping">
          <TextInput placeholder="type here" />
        </FormField> */}
                <Box direction="row" gap="medium">
                    <Button type="submit" primary label="Submit"/>
                    <Button type="reset" label="Reset" />
                    <Button label="Force Next" onClick={() => forceNext()}/>
                </Box>
            </Form>
        </Box>
    </Grid>

};

export { FuncComponent };