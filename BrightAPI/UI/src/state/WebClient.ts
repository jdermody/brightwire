import { selector } from "recoil";
import { baseUrlState } from "./BaseUrlState";

class WebClient
{
    constructor(public baseUrl: string)
    {

    }
}

export const webClientState = selector({
    key: 'webClient',
    get: ({get}) => {
        const baseUrl = get(baseUrlState);
        return new WebClient(baseUrl);
    }
})