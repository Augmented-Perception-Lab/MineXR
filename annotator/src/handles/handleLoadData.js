import { collectionGroup, where, collection, doc, addDoc, getDocs, query } from "firebase/firestore";
import { firestore } from "../firebase";
import { Func } from "../Func.js";

const participants = new Set();
const envs = new Set();
const tasks = new Set();
const allFuncs = [];

// Add PIDs of any participants to exclude from annotation, e.g., done with annotation
const excludeParticipants = [];

const handleLoadData = async (setParticipants, setFuncs, setFuncIdx, setLastFuncIdx) => {
  const funcs = query(collectionGroup(firestore, 'funcs'));
  const querySnapshot = await getDocs(funcs);
  querySnapshot.forEach((doc) => {
    const funcRef = doc.ref;
    const taskRef = funcRef.parent.parent;        // taskRef.id to get the task name
    const envRef = taskRef.parent.parent;
    const participantRef = envRef.parent.parent;
    if (!excludeParticipants.includes(participantRef.id)) {
      participants.add(participantRef.id);
      envs.add(envRef);
      tasks.add(taskRef);

      console.log(participantRef.id, ' / ', envRef.id, ' / ', taskRef.id, ' / ', funcRef.id, ' => ', doc.data());
      const anchorId = doc.data().anchorId;
      const documentPath = doc.ref.path;
      const widgetImagePath = doc.data().componentImage;
      const func = new Func(participantRef.id, envRef.id, taskRef.id, anchorId, documentPath, widgetImagePath);
      console.log(func);
      allFuncs.push(func);
    // console.log(doc.id, ' => ', doc.data());
    }
  })

  console.log("loaded funcs");
  console.log(allFuncs.length);
  console.log(allFuncs);
  setParticipants(Array.from(participants));
  setFuncs(allFuncs);
  setFuncIdx(0);
  setLastFuncIdx(0);
}

const addNewData = async (funcs, setFuncs) => {
  const funcsQuery = query(collectionGroup(firestore, 'funcs'));
  const querySnapshot = await getDocs(funcsQuery);
  let newData = [];
  querySnapshot.forEach((doc) => {
    const funcRef = doc.ref;
    const taskRef = funcRef.parent.parent;        // taskRef.id to get the task name
    const envRef = taskRef.parent.parent;
    const participantRef = envRef.parent.parent;

    if (!excludeParticipants.includes(participantRef.id)) {
      const anchorId = doc.data().anchorId;
      if (funcs.filter(func => func.anchorId == anchorId).length > 0) {
        return;
      }
      const documentPath = doc.ref.path;
      const widgetImagePath = doc.data().componentImage;
      // const func = new Func(participantRef.id, envRef.id, taskRef.id, anchorId, documentPath, widgetImagePath);
      console.log(participantRef.id, ' / ', envRef.id, ' / ', taskRef.id, ' / ', funcRef.id, ' => ', doc.data());
      const func = new Func(participantRef.id, envRef.id, taskRef.id, anchorId, documentPath, widgetImagePath);
      newData.push(func);
    }
  });

  console.log(allFuncs);
  console.log([...funcs, ...newData])
  setFuncs([...funcs, ...newData]);
}

const correctData = async () => {
  // Correct wrongly named participant, env, and task.
  const q = query(collection(firestore, "/participants/Me/envs/Dungeon/tasks/Testing/funcs"),
    where("anchorId", "in",
      // ["1837", "1838",
      // "1839", "1840", "1841", "1845", "1846", "1847", "1848", "1849"]));
      ["1850", "1853", "1854", "1855", "1856", "1857", "1858"]));

  console.log("correct data");
  const querySnapshot = await getDocs(q);
  querySnapshot.forEach(async (doc) => {
    console.log(doc.ref.path, " => " , doc.data());
    let newPath = doc.ref.path;
    newPath = newPath.replace("Me", "P10");
    newPath = newPath.replace("Dungeon", "CoffeeShop");
    newPath = newPath.replace("Testing", "Working");
    let lastSlash = newPath.lastIndexOf("/");
    newPath = newPath.substring(0, lastSlash);
    // console.log(newPath);

    let dataCopy = doc.data();

    dataCopy.componentImage = dataCopy.componentImage.replace("Me", "P10");
    console.log("NEW: ", newPath, " ==> ", dataCopy);

    const docRef = await addDoc(collection(firestore, newPath), dataCopy);
    console.log("document written with ID: ", docRef.id);
  })
}

export {handleLoadData, correctData, addNewData}