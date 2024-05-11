import { Box, PageHeader, Grid, Text, Heading } from "grommet";
import {participants, envs, tasks, categories, categoryColors, uiTypeColors, taskDict, taskCategories, uiTypes} from "./settings";
import { arr } from "./arr-stats";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js';
import {Bar} from "react-chartjs-2";
import CSVSelector from "./CSVSelector";


const AnalysisBoard = (props) => {
  ChartJS.register(
    CategoryScale,
    LinearScale,
    BarElement,
    Title,
    Tooltip,
    Legend
  );

  const getCategoryDist = () => {
    const labels = categories;
    let categoryData = [];  // categoryData.length == number of categories
    let colorIdx = 0;

    categoryData.push({
      label: "",
      data: labels.map(category => props.annotations.filter(ann => ann.appCategory == category).length),
      backgroundColor: categoryColors[colorIdx]
    });

    const data = {
      labels,
      datasets: categoryData
    }
    console.log("chart data: ", data);
    return data
  }

  const getCategoryDistPerEnv = () => {
    const labels = envs;
    let categoryData = [];  // categoryData.length == number of categories
    let colorIdx = 0;
    categories.map((category, idx) => {
      const categoryAnnos = props.annotations.filter(ann => ann.appCategory == category);

      if (categoryAnnos.length < 1) {
        return
      }

      // console.log("color: ", categoryColors[idxc]);
      categoryData.push({
        label: category,
        data: labels.map(env => categoryAnnos.filter(ann => ann.envId == env).length),
        backgroundColor: categoryColors[colorIdx]
      });
      colorIdx += 1;
    })

    const data = {
      labels,
      datasets: categoryData
    }
    console.log("chart data: ", data);
    return data
  }

  const getCategoryDistPerTask = () => {
    // const labels = tasks;
    const labels = Object.keys(taskCategories);
    let categoryData = [];  // categoryData.length == number of categories
    let colorIdx = 0;
    categories.map((category, idx) => {
      const categoryAnnos = props.annotations.filter(ann => ann.appCategory == category);

      if (categoryAnnos.length < 1) {
        return
      }

      // console.log("color: ", categoryColors[idxc]);
      categoryData.push({
        label: category,
        data: labels.map(task => categoryAnnos.filter(ann => taskDict[ann.taskId] == task).length),
        backgroundColor: categoryColors[colorIdx]
      });
      colorIdx += 1;
    })

    const data = {
      labels,
      datasets: categoryData
    }
    console.log("chart data: ", data);
    return data
  }

  const fontSize = 28;

  const categoryDistOptions = {
    plugins: {
      title: {
        display: false,
        text: 'category distribution per environment',
        font: {
          size: 28
        }
      },
      legend: {
        labels: {
          color: '#000000',
          font: {
            size: fontSize,
          },
        },
      },
    },
    layout: {
      padding: {
        left: 50,
      }
    },
    responsive: true,
    scales: {
      x: {
        stacked: true,
        ticks: {
          color: '#000000',
          font: {
            size: fontSize
          }
        }
      },
      y: {
        stacked: true,
        ticks: {
          color: '#000000',
          font: {
            size: fontSize
          }
        }
      },
    },
  };

  const getUiTypeDist = () => {
    const labels = uiTypes;
    let uiTypeData = [];  // categoryData.length == number of categories
    let colorIdx = 0;

    uiTypeData.push({
      label: "",
      data: labels.map(uiType => props.annotations.filter(ann => ann.uiTypes == uiType).length),
      backgroundColor: uiTypeColors[colorIdx]
    });

    const data = {
      labels,
      datasets: uiTypeData
    }
    console.log("chart data: ", data);
    return data
  }

  const getUiTypeDistPerEnv = () => {
    // const labels = tasks;
    const labels = envs;
    let uiTypeData = [];  // categoryData.length == number of categories
    let colorIdx = 0;
    uiTypes.map((uiType, idx) => {
      const uiTypeAnnos = props.annotations.filter(ann => ann.uiTypes == uiType);

      if (uiTypeAnnos.length < 1) {
        return
      }

      // console.log("color: ", categoryColors[idxc]);
      uiTypeData.push({
        label: uiType,
        data: labels.map(env => uiTypeAnnos.filter(ann => ann.envId == env).length),
        backgroundColor: uiTypeColors[colorIdx]
      });
      colorIdx += 1;
    })

    const data = {
      labels,
      datasets: uiTypeData
    }
    console.log("chart data: ", data);
    return data
  }

  const getUiTypeDistPerTask = () => {
    // const labels = tasks;
    const labels = Object.keys(taskCategories);
    let uiTypeData = [];  // categoryData.length == number of categories
    let colorIdx = 0;
    uiTypes.map((uiType, idx) => {
      const uiTypeAnnos = props.annotations.filter(ann => ann.uiTypes == uiType);

      if (uiTypeAnnos.length < 1) {
        return
      }

      // console.log("color: ", categoryColors[idxc]);
      uiTypeData.push({
        label: uiType,
        data: labels.map(task => uiTypeAnnos.filter(ann => taskDict[ann.taskId] == task).length),
        backgroundColor: uiTypeColors[colorIdx]
      });
      colorIdx += 1;
    })

    const data = {
      labels,
      datasets: uiTypeData
    }
    console.log("chart data: ", data);
    return data
  }

  const getPerPerson = () => {
    // annotations.filter(ann => ann.pId == );
    let pCropCounts = [];
    let pAppCounts = [];
    let pScreenshotCounts = [];

    participants.map(participant => {
      // console.log(annotations);
      let pCrops = props.annotations.filter(ann => ann.pId == participant);

      // Number of crops for this participant
      pCropCounts.push(pCrops.length);

      let pApps = [];
      let pScreenshots = [];
      pCrops.map(ann => {
        if (!pApps.includes(ann.appName)) {
          pApps.push(ann.appName);
        }
        if (!pScreenshots.includes(ann.screenshotImagePath)) {
          pScreenshots.push(ann.screenshotImagePath);
        }
      });

      pAppCounts.push(pApps.length);
      pScreenshotCounts.push(pScreenshots.length);
    });

    console.log("pCropCounts: ", pCropCounts);
    console.log("pAppCounts: ", pAppCounts);

    const avgApp = arr.average(pAppCounts);
    const minApp = arr.min(pAppCounts);
    const maxApp = arr.max(pAppCounts);
    const sdApp = arr.standardDeviation(pAppCounts);
    const sumApp = arr.sum(pAppCounts)

    const avgCrop = arr.average(pCropCounts);
    const minCrop = arr.min(pCropCounts);
    const maxCrop = arr.max(pCropCounts);
    const sdCrop = arr.standardDeviation(pCropCounts);
    const sumCrop = arr.sum(pCropCounts)

    const avgScreenshot = arr.average(pScreenshotCounts);
    const minScreenshot = arr.min(pScreenshotCounts);
    const maxScreenshot = arr.max(pScreenshotCounts);
    const sdScreenshot = arr.standardDeviation(pScreenshotCounts);
    const sumScreenshot = arr.sum(pScreenshotCounts)

    return { 
      avgApp: avgApp, minApp: minApp, maxApp: maxApp, sdApp: sdApp, sumApp: sumApp,
      avgCrop: avgCrop, minCrop: minCrop, maxCrop: maxCrop, sdCrop: sdCrop, sumCrop: sumCrop,
      avgScreenshot: avgScreenshot, minScreenshot: minScreenshot, maxScreenshot: maxScreenshot,
      sdScreenshot: sdScreenshot, sumScreenshot: sumScreenshot,
    };
  };

  const getPerEnv = () => {
    // annotations.filter(ann => ann.pId == );
    let pCropCounts = [];
    let pAppCounts = [];
    let pScreenshotCounts = [];

    participants.map(participant => {
      envs.map(env => {
        let pCrops = props.annotations.filter(ann => ann.pId == participant && ann.envId == env);

        // Number of crops for this participant
        pCropCounts.push(pCrops.length);

        let pApps = [];
        let pScreenshots = [];
        pCrops.map(ann => {
          if (!pApps.includes(ann.appName)) {
            pApps.push(ann.appName);
          }
          if (!pScreenshots.includes(ann.screenshotImagePath)) {
            pScreenshots.push(ann.screenshotImagePath);
          }
        });

        pAppCounts.push(pApps.length);
        pScreenshotCounts.push(pScreenshots.length);
      })
    });

    pCropCounts = pCropCounts.filter(e => e !== 0);
    pAppCounts = pAppCounts.filter(e => e !== 0);
    pScreenshotCounts = pScreenshotCounts.filter(e => e !== 0);

    console.log("pCropCounts: ", pCropCounts);
    console.log("pAppCounts: ", pAppCounts);

    const avgApp = arr.average(pAppCounts);
    const minApp = arr.min(pAppCounts);
    const maxApp = arr.max(pAppCounts);
    const sdApp = arr.standardDeviation(pAppCounts);
    const sumApp = arr.sum(pAppCounts)

    const avgCrop = arr.average(pCropCounts);
    const minCrop = arr.min(pCropCounts);
    const maxCrop = arr.max(pCropCounts);
    const sdCrop = arr.standardDeviation(pCropCounts);
    const sumCrop = arr.sum(pCropCounts);

    const avgScreenshot = arr.average(pScreenshotCounts);
    const minScreenshot = arr.min(pScreenshotCounts);
    const maxScreenshot = arr.max(pScreenshotCounts);
    const sdScreenshot = arr.standardDeviation(pScreenshotCounts);
    const sumScreenshot = arr.sum(pScreenshotCounts)

    return {
      avgApp: avgApp, minApp: minApp, maxApp: maxApp, sdApp: sdApp, sumApp: sumApp,
      avgCrop: avgCrop, minCrop: minCrop, maxCrop: maxCrop, sdCrop: sdCrop, sumCrop: sumCrop,
      avgScreenshot: avgScreenshot, minScreenshot: minScreenshot, maxScreenshot: maxScreenshot,
      sdScreenshot: sdScreenshot, sumScreenshot: sumScreenshot,
    };
  };

  const getPerTask = () => {
    // annotations.filter(ann => ann.pId == );
    let pCropCounts = [];
    let pAppCounts = [];
    let pScreenshotCounts = [];

    participants.map(participant => {
      tasks.map(task => {
        // console.log(annotations);
        let pCrops = props.annotations.filter(ann => ann.pId == participant &&  ann.taskId == task);

        // Number of crops for this participant
        pCropCounts.push(pCrops.length);

        let pApps = [];
        let pScreenshots = [];
        pCrops.map(ann => {
          if (!pApps.includes(ann.appName)) {
            pApps.push(ann.appName);
          }
          if (!pScreenshots.includes(ann.screenshotImagePath)) {
            pScreenshots.push(ann.screenshotImagePath);
          }
        });

        pAppCounts.push(pApps.length);
        pScreenshotCounts.push(pScreenshots.length);
      });
    });

    pCropCounts = pCropCounts.filter(e => e !== 0);
    pAppCounts = pAppCounts.filter(e => e !== 0);
    pScreenshotCounts = pScreenshotCounts.filter(e => e !== 0);

    console.log("pCropCounts: ", pCropCounts);
    console.log("pAppCounts: ", pAppCounts);

    const avgApp = arr.average(pAppCounts);
    const minApp = arr.min(pAppCounts);
    const maxApp = arr.max(pAppCounts);
    const sdApp = arr.standardDeviation(pAppCounts);
    const sumApp = arr.sum(pAppCounts)

    const avgCrop = arr.average(pCropCounts);
    const minCrop = arr.min(pCropCounts);
    const maxCrop = arr.max(pCropCounts);
    const sdCrop = arr.standardDeviation(pCropCounts);
    const sumCrop = arr.sum(pCropCounts)

    const avgScreenshot = arr.average(pScreenshotCounts);
    const minScreenshot = arr.min(pScreenshotCounts);
    const maxScreenshot = arr.max(pScreenshotCounts);
    const sdScreenshot = arr.standardDeviation(pScreenshotCounts);
    const sumScreenshot = arr.sum(pScreenshotCounts)

    return {
      avgApp: avgApp, minApp: minApp, maxApp: maxApp, sdApp: sdApp, sumApp: sumApp,
      avgCrop: avgCrop, minCrop: minCrop, maxCrop: maxCrop, sdCrop: sdCrop, sumCrop: sumCrop,
      avgScreenshot: avgScreenshot, minScreenshot: minScreenshot, maxScreenshot: maxScreenshot,
      sdScreenshot: sdScreenshot, sumScreenshot: sumScreenshot,
    };
  };

  return (
    <Box>
      <Box margin="medium"
           pad="medium"
           border={{ color: 'brand', size: 'large' }}>
        <Box  alignSelf="start">
          <CSVSelector onChange={(_data) => props.setAnnotations(_data)}/>
        </Box>
        <PageHeader title="basic analysis" />
        <Grid columns={['medium', 'medium', 'medium']}
              rows={['medium', 'medium', 'medium']}
              gap="medium"
              >
          <Box>
            <Heading level="2">number of apps per person</Heading>
            <Text color="brand" weight="bold">average: </Text><Text>{getPerPerson().avgApp}</Text>
            <Text color="brand" weight="bold">standard deviation: </Text><Text>{getPerPerson().sdApp}</Text>
            <Text color="brand" weight="bold">min: </Text><Text>{getPerPerson().minApp}</Text>
            <Text color="brand" weight="bold">max: </Text><Text>{getPerPerson().maxApp}</Text>
            <Text color="brand" weight="bold">sum: </Text><Text>{getPerPerson().sumApp}</Text>
          </Box>
          <Box>
            <Heading level="2">number of screenshots per person</Heading>
            <Text color="brand" weight="bold">average: </Text><Text>{getPerPerson().avgScreenshot}</Text>
            <Text color="brand" weight="bold">standard deviation: </Text><Text>{getPerPerson().sdScreenshot}</Text>
            <Text color="brand" weight="bold">min: </Text><Text>{getPerPerson().minScreenshot}</Text>
            <Text color="brand" weight="bold">max: </Text><Text>{getPerPerson().maxScreenshot}</Text>
            <Text color="brand" weight="bold">sum: </Text><Text>{getPerPerson().sumScreenshot}</Text>
          </Box>
          <Box>
            <Heading level="2">number of crops per person</Heading>
            <Text color="brand" weight="bold">average: </Text><Text>{getPerPerson().avgCrop}</Text>
            <Text color="brand" weight="bold">standard deviation: </Text><Text>{getPerPerson().sdCrop}</Text>
            <Text color="brand" weight="bold">min: </Text><Text>{getPerPerson().minCrop}</Text>
            <Text color="brand" weight="bold">max: </Text><Text>{getPerPerson().maxCrop}</Text>
            <Text color="brand" weight="bold">sum: </Text><Text>{getPerPerson().sumCrop}</Text>
          </Box>

          <Box>
            <Heading level="2">number of apps per environment
              (per person)</Heading>
            <Text color="brand" weight="bold">average: </Text><Text>{getPerEnv().avgApp}</Text>
            <Text color="brand" weight="bold">standard deviation: </Text><Text>{getPerEnv().sdApp}</Text>
            <Text color="brand" weight="bold">min: </Text><Text>{getPerEnv().minApp}</Text>
            <Text color="brand" weight="bold">max: </Text><Text>{getPerEnv().maxApp}</Text>
            <Text color="brand" weight="bold">sum: </Text><Text>{getPerEnv().sumApp}</Text>
          </Box>
          <Box>
            <Heading level="2">number of screenshots per environment (per person)</Heading>
            <Text color="brand" weight="bold">average: </Text><Text>{getPerEnv().avgScreenshot}</Text>
            <Text color="brand" weight="bold">standard deviation: </Text><Text>{getPerEnv().sdScreenshot}</Text>
            <Text color="brand" weight="bold">min: </Text><Text>{getPerEnv().minScreenshot}</Text>
            <Text color="brand" weight="bold">max: </Text><Text>{getPerEnv().maxScreenshot}</Text>
            <Text color="brand" weight="bold">sum: </Text><Text>{getPerEnv().sumScreenshot}</Text>
          </Box>
          <Box>
            <Heading level="2">number of crops per environment (per person)</Heading>
            <Text color="brand" weight="bold">average: </Text><Text>{getPerEnv().avgCrop}</Text>
            <Text color="brand" weight="bold">standard deviation: </Text><Text>{getPerEnv().sdCrop}</Text>
            <Text color="brand" weight="bold">min: </Text><Text>{getPerEnv().minCrop}</Text>
            <Text color="brand" weight="bold">max: </Text><Text>{getPerEnv().maxCrop}</Text>
            <Text color="brand" weight="bold">sum: </Text><Text>{getPerEnv().sumCrop}</Text>
          </Box>

          <Box>
            <Heading level="2">number of apps per task (per person)</Heading>
            <Text color="brand" weight="bold">average: </Text><Text>{getPerTask().avgApp}</Text>
            <Text color="brand" weight="bold">standard deviation: </Text><Text>{getPerTask().sdApp}</Text>
            <Text color="brand" weight="bold">min: </Text><Text>{getPerTask().minApp}</Text>
            <Text color="brand" weight="bold">max: </Text><Text>{getPerTask().maxApp}</Text>
            <Text color="brand" weight="bold">sum: </Text><Text>{getPerTask().sumApp}</Text>
          </Box>
          <Box>
            <Heading level="2">number of screenshots per task (per person)</Heading>
            <Text color="brand" weight="bold">average: </Text><Text>{getPerTask().avgScreenshot}</Text>
            <Text color="brand" weight="bold">standard deviation: </Text><Text>{getPerTask().sdScreenshot}</Text>
            <Text color="brand" weight="bold">min: </Text><Text>{getPerTask().minScreenshot}</Text>
            <Text color="brand" weight="bold">max: </Text><Text>{getPerTask().maxScreenshot}</Text>
            <Text color="brand" weight="bold">sum: </Text><Text>{getPerTask().sumScreenshot}</Text>
          </Box>
          <Box>
            <Heading level="2">number of crops per task (per person)</Heading>
            <Text color="brand" weight="bold">average: </Text><Text>{getPerTask().avgCrop}</Text>
            <Text color="brand" weight="bold">standard deviation: </Text><Text>{getPerTask().sdCrop}</Text>
            <Text color="brand" weight="bold">min: </Text><Text>{getPerTask().minCrop}</Text>
            <Text color="brand" weight="bold">max: </Text><Text>{getPerTask().maxCrop}</Text>
            <Text color="brand" weight="bold">sum: </Text><Text>{getPerTask().sumCrop}</Text>
          </Box>
        </Grid>

      </Box>
      <Box margin="medium" pad="medium"
           border={{ color: 'brand', size: 'large' }}>
        <PageHeader title="distribution of categories" />

        <Box style={{ marginBottom: "100px"}}>
        <Box margin="large">
          <Heading level="2">category distribution</Heading>
          <Bar type='bar' options={categoryDistOptions} data={getCategoryDist()} />
        </Box>
        <Box margin="large">
          <Heading level="2">category distribution per environment</Heading>
          <Bar type='bar' options={categoryDistOptions} data={getCategoryDistPerEnv()} />
        </Box>
        <Box margin="large">
          <Heading level="2">category distribution per task</Heading>
          <Bar type='bar' options={categoryDistOptions} data={getCategoryDistPerTask()} />
        </Box>
        </Box>
      </Box>
      <Box margin="medium" pad="medium"
           border={{ color: 'brand', size: 'large' }}>
        <PageHeader title="distribution of UI types" />
        <Box style={{ marginBottom: "100px"}}>
          <Box margin="large">
            <Heading level="2">UI type distribution</Heading>
            <Bar type='bar' options={categoryDistOptions} data={getUiTypeDist()} />
          </Box>
          <Box margin="large">
            <Heading level="2">UI type distribution per environment</Heading>
            <Bar type='bar' options={categoryDistOptions} data={getUiTypeDistPerEnv()} />
          </Box>
          <Box margin="large">
            <Heading level="2">UI type distribution per task</Heading>
            <Bar type='bar' options={categoryDistOptions} data={getUiTypeDistPerTask()} />
          </Box>
        </Box>
      </Box>
    </Box>
  )
}

export { AnalysisBoard }