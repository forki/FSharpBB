/// Functions for managing the Suave web server.
module ServerCode.WebServer

open System.IO
open Suave
open Suave.Logging
open System.Net
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open FSharpBB.Server.Api

// Fire up our web server!
let start clientPath port =
    if not (Directory.Exists clientPath) then
        failwithf "Client-HomePath '%s' doesn't exist." clientPath

    let logger = Logging.Targets.create Logging.Info [| "Suave" |]
    let serverConfig =
        { defaultConfig with
            autoGrow = true
            logger = Targets.create LogLevel.Fatal [|"ServerCode"; "Server" |]
            homeFolder = Some clientPath
            bindings = [ HttpBinding.create HTTP (IPAddress.Parse "0.0.0.0") port] }

    let app =
        choose [
            GET >=> choose [
                path "/" >=> Files.browseFileHome "index.html"
                path "/test" >=> Successful.OK "Hello world!"
                pathRegex @"/(public|js|css|Images)/(.*)\.(css|png|gif|jpg|js|map)" >=> Files.browseHome

                path "/api/wishlist/" >=> WishList.getWishList
            ]

            POST >=> choose [
                path "/api/users/login" >=> Auth.login
                path "/api/wishlist/" >=> WishList.postWishList
            ]

            NOT_FOUND "Page not found."

        ] // >=> logWithLevelStructured Logging.Info logger logFormatStructured

    // Route.start ()
    startWebServer serverConfig (Successful.OK "Hello World!")

