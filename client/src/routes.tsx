import { createBrowserRouter } from "react-router-dom";
import Layout from "./Layout";
import SpellsPage from "./pages/SpellsPage";
import AdminPage from "./pages/AdminPage";

export const router = createBrowserRouter([
    {
        path: "/",
        element: <Layout />,
        children: [
            { index: true, element: <SpellsPage /> },
            { path: "admin", element: <AdminPage /> },
        ],
    },
]);
