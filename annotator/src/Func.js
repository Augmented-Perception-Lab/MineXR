
function getScreenshotImagePath (widgetImagePath) {
  if (widgetImagePath) {
    const cutIndex = widgetImagePath.split(".")[0].lastIndexOf("_");
    const pathTokens = widgetImagePath.substring(0, cutIndex).split("/") ;

    const screenshotImagePath = pathTokens[0] + "/screenshots/" + pathTokens[2] + ".png";
    console.log("Widget: ", widgetImagePath);
    console.log("Screenshot: ", screenshotImagePath);

    return screenshotImagePath;
  }

  return "";
}

class Func {
  constructor(pId, envId, taskId, anchorId, documentPath, widgetImagePath) {
    this.pId = pId;
    this.envId = envId;
    this.taskId = taskId;
    this.anchorId = anchorId;
    this.documentPath = documentPath;
    this.widgetImagePath = widgetImagePath;
    this.screenshotImagePath = getScreenshotImagePath(widgetImagePath);
  } 
}

export { Func }