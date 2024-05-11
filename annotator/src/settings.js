const participants = ["P01", "P02", "P03", "P04", "P05", "P06", "P07", "P08", "P09", "P10"];
const envs = ["Office", "LivingRoom", "CoffeeShop", "Kitchen"];
const tasks = ["BakingWithFriends", "CasualChat", "ChatWithFriends", "CoffeeMaking", "Cooking", "CookingWithFriends",
  "Dining", "Discussion", "FocusWork", "HangoutWithFriend", "InPersonMeeting", "MakingCoffee", "Morning", "Relaxing",
  "WatchingVideo", "Working", "WorkingAlone", "WorkingWithFriend", "WorkingWithFriends", "Workout"];

const categories = ["Books",  "Business", "Developer Tools", "Education", "Entertainment",
  "Finance", "Food & Drink", "Games", "Graphics & Design", "Health & Fitness",
  "Kids", "Lifestyle",
  "Magazines & Newspapers", "Medical", "Music", "Navigation", "News",
  "Photo & Video", "Productivity", "Reference",
  "Shopping", "Social Networking", "Sports", "Travel", "Utilities", "Weather"
];

const uiTypes = ["Input Controls", "Navigational Components", "Informational Components", "Containers"];

const categoryColors =


  ["rgb(60,180,75)",
  "rgb(255,225,25)",
  "rgb(67,99,216)",
  "rgb(245,130,49)",
  "rgb(255,250,200)",
  "rgb(145,30,180)",
  "rgb(70,240,240)",
  "rgb(240,50,230)",
  "rgb(188,246,12)",
  "rgb(250,190,190)",
  "rgb(0,128,128)",
  "rgb(230,190,255)",
  "rgb(154,99,36)",
  "rgb(170,255,195)",
  "rgb(128,0,0)",
  "rgb(128,128,0)",
    "rgb(0,0,117)",
  "rgb(255,216,177)",
  "rgb(230,25,75)",
  "rgb(128,128,128)",
  "rgb(0,0,0)"]

const uiTypeColors = ["rgb(0,54,255)",
  "rgb(141,163,246)",
  "rgb(244,119,127)",
  "rgb(147,0,58)"]

const taskDict = {
  "BakingWithFriends": "CookingWithFriends",  // Kitchen
  "CookingWithFriends": "CookingWithFriends", // Kitchen

  "CasualChat": "CasualChat", // Office, CoffeeShop
  "ChatWithFriends": "CasualChat",  // Office, CoffeeShop
  "HangoutWithFriend": "CasualChat",

  "CoffeeMaking": "MorningCoffee",  // Kitchen
  "MakingCoffee": "MorningCoffee",
  "Morning": "MorningCoffee",

  "Cooking": "Cooking",

  "Workout": "Workout",

  "Dining": "Dining",

  "InPersonMeeting": "InPersonMeeting",

  "Discussion": "RemoteMeeting",

  "Working": "Working",
  "FocusWork": "Working",
  "WorkingAlone": "Working",

  "WorkingWithFriend": "Coworking",
  "WorkingWithFriends": "Coworking",

  "Relaxing": "Relaxing",
  "WatchingVideo": "Relaxing",
}

const taskCategories = {
  "Cooking": ["Cooking"],
  "CookingWithFriends": ["BakingWithFriends", "CookingWithFriends"],
  "CasualChat": ["CasualChat", "ChatWithFriends", "HangoutWithFriend"],
  "MorningCoffee": ["CoffeeMaking", "MakingCoffee", "Morning"],
  "Workout": ["Workout"],
  "Dining": ["Dining"],
  "InPersonMeeting": ["InPersonMeeting"],
  "RemoteMeeting": ["Discussion"],
  "Working": ["Working", "FocusWork", "WorkingAlone"],
  "Coworking": ["WorkingWithFriend", "WorkingWithFriends"],
  "Relaxing": ["Relaxing", "WatchingVideo"]
}

export { participants, envs, tasks, taskDict, taskCategories, categories, categoryColors, uiTypes, uiTypeColors };