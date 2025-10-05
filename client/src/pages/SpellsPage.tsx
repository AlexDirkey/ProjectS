import { useEffect, useMemo, useRef, useState } from "react";

// Peg mod dit .NET API (kan overrides via .env: VITE_API_BASE=http://localhost:5080)
const API_BASE =
    // @ts-expect-error Vite env
    (import.meta as any)?.env?.VITE_API_BASE || "http://localhost:5080";

// --- Typer der matcher din backend ---
export type ClassResponse = { id: string; name: string };
export type PagedResult<T> = { items: T[]; total: number; page: number; pageSize: number };

export type SpellResponse = {
    id: string;
    name: string;
    level: number;
    schoolId?: string | null;
    castingTime?: string | null;
    range?: string | null;
    components?: string | null;
    duration?: string | null;
    concentration?: boolean;
    ritual?: boolean;
    description?: string | null;
    higherLevel?: string | null;
    classNames?: string[];
};

// --- Helpers ---
function useDebounced<T>(value: T, delay = 300) {
    const [v, setV] = useState<T>(value);
    useEffect(() => {
        const t = setTimeout(() => setV(value), delay);
        return () => clearTimeout(t);
    }, [value, delay]);
    return v;
}

async function api<T>(path: string, init?: RequestInit): Promise<T> {
    const res = await fetch(`${API_BASE}${path}`, {
        ...init,
        headers: { "Content-Type": "application/json", ...(init?.headers || {}) },
    });
    if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
    return res.json();
}

function Field({ label, value }: { label: string; value?: string | boolean | null }) {
    return (
        <div style={{ fontSize: 14 }}>
            <div style={{ color: "#64748b", fontSize: 12 }}>{label}</div>
            <div>{value ? String(value) : "—"}</div>
        </div>
    );
}

export default function SpellsPage() {
    // filters
    const [classes, setClasses] = useState<ClassResponse[]>([]);
    const [selectedClass, setSelectedClass] = useState<string>("");
    const [search, setSearch] = useState<string>("");
    const q = useDebounced(search, 300);
    const [level, setLevel] = useState<string>("");

    // paging
    const [page, setPage] = useState<number>(1);
    const [pageSize, setPageSize] = useState<number>(20);

    // data
    const [spells, setSpells] = useState<SpellResponse[]>([]);
    const [total, setTotal] = useState<number>(0);
    const [loading, setLoading] = useState<boolean>(false);
    const [err, setErr] = useState<string | null>(null);

    const totalPages = useMemo(() => Math.max(1, Math.ceil(total / pageSize)), [total, pageSize]);

    // details
    const [active, setActive] = useState<SpellResponse | null>(null);

    // load classes once
    useEffect(() => {
        let mounted = true;
        api<ClassResponse[]>(`/api/Class`)
            .then((data) => mounted && setClasses(data))
            .catch((e: any) => mounted && setErr(e?.message ?? "Kunne ikke hente klasser"));
        return () => {
            mounted = false;
        };
    }, []);

    // load spells when filters change
    useEffect(() => {
        const c = new AbortController();
        setLoading(true);
        setErr(null);

        const params = new URLSearchParams();
        if (q.trim()) params.set("q", q.trim());
        if (selectedClass) params.set("classId", selectedClass);
        if (level) params.set("level", level);
        params.set("page", String(page));
        params.set("pageSize", String(pageSize));

        api<PagedResult<SpellResponse>>(`/api/Spell?${params.toString()}`, { signal: c.signal })
            .then((res) => {
                setSpells(res.items || []);
                setTotal(res.total || 0);
            })
            .catch((e: any) => {
                if (e?.name !== "AbortError") setErr(e?.message ?? "Kunne ikke hente spells");
            })
            .finally(() => setLoading(false));

        return () => c.abort();
    }, [q, selectedClass, level, page, pageSize]);

    // reset page when filters change
    const first = useRef<boolean>(true);
    useEffect(() => {
        if (first.current) {
            first.current = false;
            return;
        }
        setPage(1);
    }, [q, selectedClass, level]);

    return (
        <div style={{ padding: 16 }}>
            <header
                style={{
                    position: "sticky",
                    top: 0,
                    background: "white",
                    padding: "10px 0",
                    borderBottom: "1px solid #e2e8f0",
                    marginBottom: 12,
                    zIndex: 1,
                }}
            >
                <div
                    style={{
                        maxWidth: 1100,
                        margin: "0 auto",
                        display: "flex",
                        alignItems: "center",
                        gap: 12,
                    }}
                >
                    <strong style={{ fontSize: 18 }}>D&D 5e – Spells per Class</strong>
                    <span style={{ marginLeft: "auto", color: "#64748b", fontSize: 12 }}>API: {API_BASE}</span>
                </div>
            </header>

            <main style={{ maxWidth: 1100, margin: "0 auto" }}>
                {/* Filters */}
                <div
                    style={{
                        display: "grid",
                        gridTemplateColumns: "1fr 220px 160px 150px",
                        gap: 8,
                        marginBottom: 10,
                    }}
                >
                    <input
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        placeholder="Søg i navn/beskrivelse…"
                        style={{ padding: 8, borderRadius: 8, border: "1px solid #cbd5e1" }}
                    />

                    <select
                        value={selectedClass}
                        onChange={(e) => setSelectedClass(e.target.value)}
                        style={{ padding: 8, borderRadius: 8, border: "1px solid #cbd5e1" }}
                    >
                        <option value="">Alle klasser</option>
                        {classes.map((c) => (
                            <option key={c.id} value={c.id}>
                                {c.name}
                            </option>
                        ))}
                    </select>

                    <select
                        value={level}
                        onChange={(e) => setLevel(e.target.value)}
                        style={{ padding: 8, borderRadius: 8, border: "1px solid #cbd5e1" }}
                    >
                        <option value="">Alle levels</option>
                        {Array.from({ length: 10 }, (_, i) => i).map((lvl) => (
                            <option key={lvl} value={lvl}>
                                {lvl === 0 ? "Cantrip (0)" : `Level ${lvl}`}
                            </option>
                        ))}
                    </select>

                    <select
                        value={pageSize}
                        onChange={(e) => setPageSize(parseInt(e.target.value, 10))}
                        style={{ padding: 8, borderRadius: 8, border: "1px solid #cbd5e1" }}
                    >
                        {[10, 20, 50, 100].map((n) => (
                            <option key={n} value={n}>
                                {n} pr. side
                            </option>
                        ))}
                    </select>
                </div>

                {err && (
                    <div
                        style={{
                            padding: 8,
                            background: "#ffefef",
                            border: "1px solid #f0b7b7",
                            borderRadius: 6,
                            marginBottom: 10,
                        }}
                    >
                        {err}
                    </div>
                )}

                {/* Results */}
                <div
                    style={{
                        overflow: "hidden",
                        border: "1px solid #e2e8f0",
                        borderRadius: 12,
                        background: "white",
                    }}
                >
                    <table style={{ width: "100%", tableLayout: "fixed", fontSize: 14 }}>
                        <thead>
                        <tr style={{ background: "#f1f5f9", textAlign: "left" }}>
                            <th style={{ padding: "10px 12px", width: 80 }}>Level</th>
                            <th style={{ padding: "10px 12px" }}>Navn</th>
                            <th style={{ padding: "10px 12px" }}>Skole</th>
                            <th style={{ padding: "10px 12px" }}>Casting time</th>
                            <th style={{ padding: "10px 12px" }}>Range</th>
                            <th style={{ padding: "10px 12px", width: 130 }}>Flag</th>
                        </tr>
                        </thead>
                        <tbody>
                        {loading && (
                            <tr>
                                <td colSpan={6} style={{ padding: 16, textAlign: "center" }}>
                                    Henter data…
                                </td>
                            </tr>
                        )}

                        {!loading && spells.length === 0 && (
                            <tr>
                                <td colSpan={6} style={{ padding: 16, textAlign: "center" }}>
                                    Ingen resultater
                                </td>
                            </tr>
                        )}

                        {!loading &&
                            spells.map((s) => (
                                <tr
                                    key={s.id}
                                    style={{ borderTop: "1px solid #e2e8f0", cursor: "pointer" }}
                                    onClick={() => setActive(s)}
                                >
                                    <td style={{ padding: "10px 12px" }}>{s.level}</td>
                                    <td style={{ padding: "10px 12px" }}>
                                        <div style={{ fontWeight: 600 }}>{s.name}</div>
                                        {Array.isArray(s.classNames) && s.classNames.length > 0 && (
                                            <div
                                                style={{
                                                    marginTop: 4,
                                                    display: "flex",
                                                    flexWrap: "wrap",
                                                    gap: 4,
                                                    color: "#64748b",
                                                    fontSize: 12,
                                                }}
                                            >
                                                {s.classNames.map((cn) => (
                                                    <span
                                                        key={cn}
                                                        style={{
                                                            padding: "2px 6px",
                                                            background: "#f1f5f9",
                                                            borderRadius: 6,
                                                        }}
                                                    >
                              {cn}
                            </span>
                                                ))}
                                            </div>
                                        )}
                                    </td>
                                    <td style={{ padding: "10px 12px" }}>{s.schoolId ?? "—"}</td>
                                    <td style={{ padding: "10px 12px" }}>{s.castingTime ?? "—"}</td>
                                    <td style={{ padding: "10px 12px" }}>{s.range ?? "—"}</td>
                                    <td style={{ padding: "10px 12px" }}>
                                        <div style={{ display: "flex", flexWrap: "wrap", gap: 6 }}>
                                            {s.concentration ? (
                                                <span
                                                    style={{
                                                        padding: "2px 6px",
                                                        fontSize: 12,
                                                        background: "#eef2ff",
                                                        color: "#3730a3",
                                                        borderRadius: 6,
                                                    }}
                                                >
                            Concentration
                          </span>
                                            ) : null}
                                            {s.ritual ? (
                                                <span
                                                    style={{
                                                        padding: "2px 6px",
                                                        fontSize: 12,
                                                        background: "#ecfdf5",
                                                        color: "#065f46",
                                                        borderRadius: 6,
                                                    }}
                                                >
                            Ritual
                          </span>
                                            ) : null}
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>

                {/* Pagination */}
                <div
                    style={{
                        marginTop: 12,
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "space-between",
                        gap: 8,
                        fontSize: 14,
                    }}
                >
                    <div>
                        Viser <b>{Math.min(total, (page - 1) * pageSize + 1)}</b>–
                        <b>{Math.min(page * pageSize, total)}</b> af <b>{total}</b>
                    </div>
                    <div style={{ display: "flex", gap: 6 }}>
                        <button onClick={() => setPage(1)} disabled={page === 1}>
                            « Første
                        </button>
                        <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1}>
                            ← Forrige
                        </button>
                        <div>
                            Side {page} / {totalPages}
                        </div>
                        <button
                            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                            disabled={page >= totalPages}
                        >
                            Næste →
                        </button>
                        <button onClick={() => setPage(totalPages)} disabled={page >= totalPages}>
                            Sidste »
                        </button>
                    </div>
                </div>
            </main>

            {/* Details modal */}
            {active && (
                <div
                    onClick={() => setActive(null)}
                    style={{
                        position: "fixed",
                        inset: 0,
                        background: "rgba(0,0,0,0.35)",
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "center",
                        padding: 12,
                        zIndex: 20,
                    }}
                >
                    <div
                        onClick={(e) => e.stopPropagation()}
                        style={{
                            maxHeight: "85vh",
                            width: "min(780px, 100%)",
                            overflow: "auto",
                            background: "white",
                            borderRadius: 12,
                            boxShadow: "0 10px 30px rgba(0,0,0,0.2)",
                        }}
                    >
                        <div
                            style={{
                                borderBottom: "1px solid #e2e8f0",
                                padding: "12px 16px",
                                display: "flex",
                                alignItems: "start",
                                gap: 10,
                            }}
                        >
                            <div style={{ flex: 1 }}>
                                <div style={{ fontSize: 18, fontWeight: 600 }}>{active.name}</div>
                                <div style={{ color: "#64748b", fontSize: 13 }}>
                                    Level {active.level}
                                    {active.schoolId ? ` · ${active.schoolId}` : ""}
                                </div>
                            </div>
                            <button onClick={() => setActive(null)}>Luk</button>
                        </div>

                        <div style={{ padding: 16, display: "grid", gap: 14 }}>
                            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
                                <Field label="Casting time" value={active.castingTime} />
                                <Field label="Range" value={active.range} />
                                <Field label="Components" value={active.components} />
                                <Field label="Duration" value={active.duration} />
                                <Field label="Concentration" value={active.concentration ? "Yes" : "No"} />
                                <Field label="Ritual" value={active.ritual ? "Yes" : "No"} />
                            </div>

                            {Array.isArray(active.classNames) && active.classNames.length > 0 && (
                                <div>
                                    <div style={{ fontWeight: 600, marginBottom: 4, fontSize: 14 }}>Classes</div>
                                    <div style={{ display: "flex", gap: 6, flexWrap: "wrap" }}>
                                        {active.classNames.map((cn) => (
                                            <span
                                                key={cn}
                                                style={{ padding: "2px 6px", background: "#f1f5f9", borderRadius: 6 }}
                                            >
                        {cn}
                      </span>
                                        ))}
                                    </div>
                                </div>
                            )}

                            {active.description && (
                                <section>
                                    <h3 style={{ fontWeight: 600, marginBottom: 4 }}>Description</h3>
                                    <p style={{ whiteSpace: "pre-wrap", lineHeight: 1.5, fontSize: 14 }}>
                                        {active.description}
                                    </p>
                                </section>
                            )}

                            {active.higherLevel && (
                                <section>
                                    <h3 style={{ fontWeight: 600, marginBottom: 4 }}>At Higher Levels</h3>
                                    <p style={{ whiteSpace: "pre-wrap", lineHeight: 1.5, fontSize: 14 }}>
                                        {active.higherLevel}
                                    </p>
                                </section>
                            )}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}


