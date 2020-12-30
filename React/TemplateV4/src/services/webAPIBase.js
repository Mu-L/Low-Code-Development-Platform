import { fetchService } from './fetchService';
import { authService } from './authService';
import { getRintagiConfig } from '../helpers/config';
import log from '../helpers/logger';

const rintagi = getRintagiConfig() || {};

export const baseUrl = rintagi.apiBasename + "/webservices";
export const fetchAPIResult = fetchService.fetchAPIResult;
export const getAPIResult = fetchService.getAPIResult;
export const getAccessControlInfo = authService.getAccessControlInfo;
export const getAccessScope = authService.getAccessScope;

export function fetchData(url,options)
{
    return fetchAPIResult(url,options)
    .then(
    async result=>{
        if (result.status === "success" && result.data.value.d && result.data.value.d.status==="success") {
            return {
                data: getAPIResult(result).data,
                supportingData: getAPIResult(result).supportingData || {}
            }
        }
        else {
            return Promise.reject( {
                status : "failed",
                url:url,
                errType: result.status === "success" ? "api call error" : result.errType,
                errSubType: result.errSubType || (result.status === "success" ? result.data.value.d.status || result.data.value.d.errMsg : ""),
                errMsg : result.status === "success" ? result.data.value.d.errorMsg : result.errType,
                validationErrors: result.status === "success" ? result.data.value.d.validationErrors : null,
            })
        }
    },
    error=>{
        return Promise.reject(error);
    }
    )
}

export function ddlSuggests (url, query, contextStr, topN,accessScope) 
{
    return fetchAPIResult(baseUrl+url
    ,{
        requestOptions: {
            body:JSON.stringify({
                query:query,
                contextStr: contextStr,
                topN: topN || 50
            })
        },
        ...(getAccessControlInfo()),
        ...(accessScope)
    })
    .then(
    async result=>{
        if (result.status === "success" && result.data.value.d && result.data.value.d.status==="success") {
            return {
                data: getAPIResult(result).data,
                supportingData: getAPIResult(result).supportingData || {}
            }
        }
        else {
            return Promise.reject( {
                status : "failed",
                errType: result.status === "success" ? "api call error" : result.errType,
                errSubType: result.errSubType || result.data.value.d.error,
                errMsg : result.status === "success" ? result.data.value.d.message : result.errType
            })
        }
    },
    error=>{
        return Promise.reject(error);
    }
    )
}

export function MakeDdlServiceFn(serviceUrl, getAccessControlInfo, currentScope) {
    return function (query, topN, filterBy, accessScope){
        return fetchData(serviceUrl
            ,{
                requestOptions: {
                    body: JSON.stringify({
                        query: query || "",
                        topN: topN || 0,
                        filterBy: filterBy || null
                    }),
                },
                ...(getAccessControlInfo()),
                ...(accessScope)
            }
        )
    }
}

export function keepNullOrUndefinedFields (o)
{
    if (!o || typeof o !== "object" || true) return o;
    return Object.keys(o||{}).reduce((a,k)=>{o[k] = o[k] === undefined || o[k] === null ?  '' : o[k];  return a;},o)
};