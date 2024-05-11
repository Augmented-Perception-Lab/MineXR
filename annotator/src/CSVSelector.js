import React from "react";
import { FileInput } from "grommet";

const CSVSelector = ({ onChange }) => {
  const csvToJson = (data) => {
    const lines = data.split("\n");
    const headers = lines[0].split(",");

    const jsonArray = [];

    for (let i = 1; i < lines.length; i++) {
      const values = lines[i].split(",");
      const jsonObject = {};

      for (let j = 0; j < headers.length; j++) {
        const header = parseCSVValue(headers[j]);
        const value = parseCSVValue(values[j]);

        if (header == "funcIdx") {
          jsonObject[header] = parseInt(value);
        } else if (header.startsWith("is")) {
          jsonObject[header] = (value === 'true');
        } else {
          jsonObject[header] = value;
        }
      }

      jsonArray.push(jsonObject);
    }

    return jsonArray;
  }

  const parseCSVValue = (value) => {
    // Check if the value is an array (enclosed in square brackets)
    if (value && value.startsWith("[") && value.endsWith("]")) {
      try {
        // Parse the array using JSON.parse
        console.log("JSON PARSE VALUE");
        console.log(value);
        console.log(JSON.parse(value));
        return JSON.parse(value);
      } catch (error) {
        console.error("Error parsing array value:", error);
        return value;
      }
    } else {
      // Remove leading/trailing spaces and replace double quotes with blank
      return value ? value.trim().replace(/"/g, "") : "";
    }
  };

  const handleFileChange = async (e) => {
    if (e.target.files) {
      try {
        const file = e.target.files[0];

        // 1. create url from the file
        const fileUrl = URL.createObjectURL(file);

        // 2. use fetch API to read the file
        const response = await fetch(fileUrl);

        // 3. get the text from the response
        const text = await response.text();

        const _data = csvToJson(text);
        //
        // // 4. split the text by newline
        // const lines = text.split("\n");
        //
        // // 5. map through all the lines and split each line by comma.
        // const _data = lines.map((line) => line.split(","));

        // 6. call the onChange event
        onChange(_data);
      } catch (error) {
        console.error(error);
      }
    }
  };

  return <FileInput name="file" onChange={handleFileChange} />;
  // return <input type="file" accept=".csv" onChange={handleFileChange} />;
};

export default CSVSelector;
