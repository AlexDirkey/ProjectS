import React, { useEffect, useState } from "react";
import {
    ApiClient,
    ClassResponse,
    ClassCreate,
    SchoolResponse,
    SchoolCreate,
} from "../api/backend";

const BASE = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5080";
const api = new ApiClient(BASE);

export default function AdminPage() {
    // classes
    const [classes, setClasses] = useState<ClassResponse[]>([]);
    const [newClassId, setNewClassId] = useState("");
    const [newClassName, setNewClassName] = useState("");
    const [deleteClassId, setDeleteClassId] = useState("");

    // schools
    const [schools, setSchools] = useState<SchoolResponse[]>([]);
    const [newSchoolId, setNewSchoolId] = useState("");
    const [newSchoolName, setNewSchoolName] = useState("");
    const [deleteSchoolId, setDeleteSchoolId] = useState("");

    const [msg, setMsg] = useState<string | null>(null);
    const [err, setErr] = useState<string | null>(null);

    async function loadLookups() {
        setErr(null);
        try {
            const [cls, sch] = await Promise.all([api.classAll(), api.schoolAll()]);
            setClasses(cls);
            setSchools(sch);
        } catch (e: any) {
            setErr(e?.message ?? "Failed to load lookups");
        }
    }

    useEffect(() => { loadLookups(); }, []);

    // ----- class handlers
    async function createClass(e: React.FormEvent) {
        e.preventDefault();
        setMsg(null); setErr(null);
        const body: ClassCreate = { id: newClassId.trim(), name: newClassName.trim() };
        if (!body.id || !body.name) return;
        try {
            await api.classPOST(body);
            setNewClassId(""); setNewClassName("");
            await loadLookups();
            setMsg("Class created.");
        } catch (e: any) { setErr(e?.message ?? "Create class failed"); }
    }

    async function removeClass(e: React.FormEvent) {
        e.preventDefault();
        setMsg(null); setErr(null);
        const id = deleteClassId.trim();
        if (!id) return;
        try {
            await api.classDELETE(id);
            setDeleteClassId("");
            await loadLookups();
            setMsg("Class deleted.");
        } catch (e: any) { setErr(e?.message ?? "Delete class failed"); }
    }

    // ----- school handlers
    async function createSchool(e: React.FormEvent) {
        e.preventDefault();
        setMsg(null); setErr(null);
        const body: SchoolCreate = { id: newSchoolId.trim(), name: newSchoolName.trim() };
        if (!body.id || !body.name) return;
        try {
            await api.schoolPOST(body);
            setNewSchoolId(""); setNewSchoolName("");
            await loadLookups();
            setMsg("School created.");
        } catch (e: any) { setErr(e?.message ?? "Create school failed"); }
    }

    async function removeSchool(e: React.FormEvent) {
        e.preventDefault();
        setMsg(null); setErr(null);
        const id = deleteSchoolId.trim();
        if (!id) return;
        try {
            await api.schoolDELETE(id);
            setDeleteSchoolId("");
            await loadLookups();
            setMsg("School deleted.");
        } catch (e: any) { setErr(e?.message ?? "Delete school failed"); }
    }

    return (
        <div style={{ maxWidth: 900, margin: "0 auto" }}>
            <h1>Admin</h1>
            {err && <div style={{ padding: 8, background: "#ffefef", border: "1px solid #f0b7b7", borderRadius: 6 }}>{err}</div>}
            {msg && <div style={{ padding: 8, background: "#eef9ef", border: "1px solid #cfe9d2", borderRadius: 6 }}>{msg}</div>}

            <section style={{ marginTop: 16 }}>
                <h2>Classes</h2>
                <form onSubmit={createClass} style={{ display: "flex", gap: 8, marginBottom: 8 }}>
                    <input placeholder="id" value={newClassId} onChange={(e) => setNewClassId(e.target.value)} />
                    <input placeholder="name" value={newClassName} onChange={(e) => setNewClassName(e.target.value)} />
                    <button type="submit">Create</button>
                </form>
                <form onSubmit={removeClass} style={{ display: "flex", gap: 8, marginBottom: 16 }}>
                    <input placeholder="id to delete" value={deleteClassId} onChange={(e) => setDeleteClassId(e.target.value)} />
                    <button type="submit">Delete</button>
                </form>
                <ul>
                    {classes.map(c => <li key={c.id}><strong>{c.name}</strong> — {c.id}</li>)}
                    {!classes.length && <li>No classes.</li>}
                </ul>
            </section>

            <section style={{ marginTop: 24 }}>
                <h2>Schools</h2>
                <form onSubmit={createSchool} style={{ display: "flex", gap: 8, marginBottom: 8 }}>
                    <input placeholder="id" value={newSchoolId} onChange={(e) => setNewSchoolId(e.target.value)} />
                    <input placeholder="name" value={newSchoolName} onChange={(e) => setNewSchoolName(e.target.value)} />
                    <button type="submit">Create</button>
                </form>
                <form onSubmit={removeSchool} style={{ display: "flex", gap: 8, marginBottom: 16 }}>
                    <input placeholder="id to delete" value={deleteSchoolId} onChange={(e) => setDeleteSchoolId(e.target.value)} />
                    <button type="submit">Delete</button>
                </form>
                <ul>
                    {schools.map(s => <li key={s.id}><strong>{s.name}</strong> — {s.id}</li>)}
                    {!schools.length && <li>No schools.</li>}
                </ul>
            </section>
        </div>
    );
}
