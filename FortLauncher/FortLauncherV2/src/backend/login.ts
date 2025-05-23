﻿import { BrowserWindow } from "electron";
import { AuthData } from "./types/AuthData";
import { readTokenFromIni } from "./IniConfig";
import axios from "axios";

class UserService {
  user: AuthData | null;

  constructor() {
    this.user = null;
    // this.news = null;
  }

  async login(authData: AuthData, TOKEN: string) {
    this.user = authData;
    this.user.AccessToken = TOKEN;
  }
}

const user = new UserService();
export default user;


export async function login(
  mainWindow: BrowserWindow,
  DONTLOGINBABE = false
): Promise<AuthData | null> {
  var TOKEN = readTokenFromIni();
  try {
    const response = await axios.get(`${process.env.VITE_BACKEND_URL}/launcher/api/v1/login`, {
      headers: {
        Authorization: `${TOKEN}`
      }
    })

    if (response.data) {
        console.log(response.data);
        user.login(response.data, TOKEN);
        if(!DONTLOGINBABE)
         mainWindow!.webContents.send('IsLoggedIn', true)
        return user.user;
    }
  } catch (err) {
    console.log(err);
  }
  console.log("TOKEN " + TOKEN);
  return null;
}
