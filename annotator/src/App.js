import { Grommet, Button, Box, Form, FormField, TextInput, Text, Image, Grid,
  Data, DataFilters, DataSearch, DataSort, DataSummary, DataTable,
  Tab, Tabs
} from 'grommet';
import './App.css';

import { useState, useEffect } from 'react';

import {handleLoadData, correctData, addNewData} from './handles/handleLoadData';
import {FuncComponent} from "./FuncComponent";
// import {} from "grommet/components";
import {Checkmark} from "grommet-icons";
import {Toolbar} from "grommet/components/Toolbar";
import {AnalysisBoard} from "./AnalysisBoard";


const useLocalStorage = (storageKey, fallbackState) => {
  const [value, setValue] = useState(
      JSON.parse(localStorage.getItem(storageKey)) ?? fallbackState
  );

  useEffect(() => {
    localStorage.setItem(storageKey, JSON.stringify(value));
  }, [value, storageKey]);

  return [value, setValue];
}

function App() {
  const [participants, setParticipants] = useState([]);

  const [funcIdx, setFuncIdx] = useLocalStorage("funcIdx", -1);
  const [lastFuncIdx, setLastFuncIdx] = useLocalStorage("lastFuncIdx", -1);

  const [funcs, setFuncs] = useLocalStorage("funcs", []);
  const [annotations, setAnnotations] = useLocalStorage("annotations", []);

  const [appNames, setAppNames] = useLocalStorage("appNames", []);
  const [screenDescs, setScreenDescs] = useLocalStorage("screenDescs", []);
  const [widgetDescs, setWidgetDescs] = useLocalStorage("widgetDescs", []);
  const [excludedParts, setExcludedParts] = useLocalStorage("excludedParts", []);

  const loadDataHandler = () => {
    handleLoadData(setParticipants, setFuncs, setFuncIdx, setLastFuncIdx);
  }

  const correctDataHandler = () => {
    correctData();
  }

  const addNewDataHandler = () => {
    addNewData(funcs, setFuncs);
  }

  const jsonToCsv = (items) => {
    // console.log(json)
    // const items = json.items
    console.log(items);
    if (items.length !== 0) {
      const replacer = (key, value) => value === null ? '' : value
      const header = Object.keys(items[0])
      const csv = [
        header.join(','),
        ...items.map(row => header.map(fieldName => JSON.stringify(row[fieldName], replacer)?.replaceAll(',', ';')).join(','))
      ].join('\r\n')
      return csv
    }
    return ''
  }

  const forceNext = () => {
    setFuncIdx(funcIdx + 1);
  }

  const addAnnotation = (annotation, func) => {
    let annoIndex = annotations.findIndex(ann => ann.anchorId == func.anchorId);
    console.log("annoIndex: " + annoIndex);
    if (annoIndex > -1) {
      // Edit an annotation
      let editedAnnotations = [...annotations];
      let editedItem = {...editedAnnotations[annoIndex]};
      editedItem.appName = annotation.appName;
      editedItem.screenDesc = annotation.screenDesc;
      editedItem.widgetDesc = annotation.widgetDesc;
      editedItem.excludedParts = annotation.excludedParts;
      editedItem.isCropped = annotation.isCropped;
      editedItem.isBlurred = annotation.isBlurred;
      editedItem.isPlaceholder = annotation.isPlaceholder;
      editedItem.category = annotation.category;
      editedItem.uiTypes = annotation.uiTypes;
      editedItem.notes = annotation.notes;

      editedAnnotations[annoIndex] = editedItem;
      setAnnotations(editedAnnotations);

      setFuncIdx(lastFuncIdx);
    } else {
      // Add a new annotation
      annotation.pId = func.pId;
      annotation.envId = func.envId;
      annotation.taskId = func.taskId;
      annotation.anchorId = func.anchorId;
      annotation.documentPath = func.documentPath;
      annotation.widgetImagePath = func.widgetImagePath;
      annotation.screenshotImagePath = func.screenshotImagePath;
      annotation.funcIdx = funcIdx;

      setAnnotations(annotations => [...annotations, annotation]);

      setLastFuncIdx(funcIdx + 1);

      console.log("annoIndex: " + funcIdx);
      setFuncIdx(funcIdx + 1);
      console.log("annoIndex: " + funcIdx);
    }


    console.log("func index: " + funcIdx);
    console.log("funcs: " + funcs.length);
    console.log("FUNC DATA");
    console.log(annotations);

    // Append to app name list for autocompletion
    if (!appNames.includes(annotation.appName) && (annotation.appName !== null)) {
      setAppNames(appNames => [...appNames, annotation.appName]);
      console.log("New app name: ", annotation.appName);
    }

    // Append to screen description list for autocompletion
    if (!screenDescs.includes(annotation.screenDesc) && (annotation.screenDesc !== null)) {
      setScreenDescs(screenDescs => [...screenDescs, annotation.screenDesc]);
    }

    // Append to widget description list for autocompletion
    if (!widgetDescs.includes(annotation.widgetDesc) && (annotation.widgetDesc !== null)) {
      setWidgetDescs(widgetDescs => [...widgetDescs, annotation.widgetDesc]);
    }

    // Append to widget description list for autocompletion
    if (!excludedParts.includes(annotation.excludedParts) && (annotation.excludedParts !== null)) {
      setExcludedParts(excludedParts => [...excludedParts, annotation.excludedParts]);
    }
  }

  return (
    <Grommet full>
      <Box direction="column" pad="large">
        <Tabs>
          <Tab title="annotate">
            <Box direction="row">
              <Box margin="medium" width="small" alignSelf="center">
                <Button primary label="Import all data" onClick={loadDataHandler} />
              </Box>
              <Box  alignSelf="center">
                <Button primary label="Export all annotations"
                        href={'data:text/plain;charset=utf-8,' + encodeURIComponent(jsonToCsv(annotations))}
                        download="annotations.csv" />
              </Box>
              {/*<Box margin="medium" alignSelf="center">*/}
              {/*  <Button secondary label="Correct data"*/}
              {/*          onClick={correctDataHandler} />*/}
              {/*</Box>*/}
              <Box margin="medium" alignSelf="center">
                <Button secondary label="Update new data"
                        onClick={addNewDataHandler} />
              </Box>
            </Box>
            <Box
              direction="row"
              margin="medium"
              border={{ color: 'brand', size: 'large' }}
            >
              {/* <Box pad="xsmall"> */}
                {/* participant list */}
                {/* {participants.map((participant, idx) => {
                  return <Button secondary key={idx} label={participant} margin="xxsmall" />
                })} */}
              {/* </Box> */}
              <Box pad="medium" alignSelf="stretch" fill={true}>
                {funcIdx !== -1 &&
                <FuncComponent fill={true} func={funcs[funcIdx]}
                               annotation={annotations.filter(ann => ann.anchorId == funcs[funcIdx].anchorId).length > 0
                                 ? annotations.filter(ann => ann.anchorId == funcs[funcIdx].anchorId)[0] : null}
                               addAnnotation={addAnnotation}
                               appNames={appNames} screenshotDescs={screenDescs} widgetDescs={widgetDescs}
                               excludedParts={excludedParts}
                               forceNext={forceNext}
                />}
              </Box>
            </Box>
            <Box pad="medium">
            <Data data={annotations}>
              <Toolbar orientation="column"><DataSearch /><DataSummary /><DataFilters drop /><DataSort drop/></Toolbar>
              <DataTable
                onClickRow={({datum}) => {setFuncIdx(datum.funcIdx)}}
                columns={[
                  {
                    property: 'anchorId',
                    header: <Text>Anchor ID</Text>,
                    primary: true
                  },
                  {
                    property: 'pId',
                    header: <Text>Participant</Text>,
                    primary: true
                  },
                  {
                    property: 'envId',
                    header: <Text>Environment</Text>,
                    primary: true
                  },
                  {
                    property: 'taskId',
                    header: <Text>Task</Text>,
                    primary: true
                  },
                  {
                      property: 'appName',
                      header: <Text>Application name</Text>,
                  },
                  {
                      property: 'screenDesc',
                      header: <Text>Screenshot description</Text>,
                  },
                  {
                      property: 'widgetDesc',
                      header: <Text>Widget description</Text>,
                  },
                  {
                      property: 'excludedParts',
                      header: <Text>Excluded parts</Text>
                  },
                  {
                      property: 'isCropped',
                      header: <Text>Cropped?</Text>,
                      render: datum => (
                          <Box alignSelf="center">
                              {datum.isCropped ? <Checkmark /> : <Box/>}
                          </Box>
                      )
                  },
                  {
                      property: 'isBlurred',
                      header: <Text>Blurred?</Text>,
                      render: datum => (
                          <Box alignSelf="center">
                              {datum.isBlurred ? <Checkmark /> : <Box/>}
                          </Box>
                      )
                  },
                  {
                      property: 'isPlaceholder',
                      header: <Text>Placeholder?</Text>,
                      render: datum => (
                          <Box alignSelf="center">
                              {datum.isPlaceholder ? <Checkmark /> : <Box/>}
                          </Box>
                      )
                  },
                  {
                      property: 'appCategory',
                      header: <Text>Category</Text>
                  },
                  {
                      property: 'uiTypes',
                      header: <Text>UI Types</Text>,
                      render: datum => (
                          <Text>{datum.uiTypes && Array.isArray(datum.uiTypes) && datum.uiTypes.join(", ")}</Text>
                      )
                  },
                  {
                      property: 'notes',
                      header: <Text>Notes</Text>
                  }
                ]}
              />
            </Data>
          </Box>
          </Tab>
          <Tab title="analyze">
            <AnalysisBoard annotations={annotations} setAnnotations={(data) => setAnnotations(data)}/>
          </Tab>
        </Tabs>
      </Box>
    </Grommet>
  );
}

export default App;
