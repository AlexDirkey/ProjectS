import React from "react";
import ReactDOM from "react-dom/client";
import { RouterProvider } from "react-router-dom";
import { router } from "./routes";

const root = document.getElementById("root");
if (!root) throw new Error("Missing #root");

ReactDOM.createRoot(root).render(
    <React.StrictMode>
        <RouterProvider router={router} />
    </React.StrictMode>
);
