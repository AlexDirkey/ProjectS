import { Link, Outlet } from "react-router-dom";

export default function Layout() {
    return (
        <div style={{ padding: 16, fontFamily: "system-ui, -apple-system, Segoe UI, Roboto, sans-serif" }}>
            <nav style={{ display: "flex", gap: 12, marginBottom: 16 }}>
                <Link to="/">Spells</Link>
                <Link to="/admin">Admin</Link>
            </nav>
            <Outlet />
        </div>
    );
}

