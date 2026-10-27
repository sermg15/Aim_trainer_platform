const $ = (s) => document.querySelector(s);
const state = { sessions: [], selected: new Set(), compareId: '', eventType: 'all' };
const aliases = {
  sessionId: ['sessionid','session','partida','gameid','matchid','idpartida'],
  timestamp: ['timestamp','datetime','fecha','time','eventtime'],
  eventType: ['eventtype','event','tipo','result','resultado','type'],
  reactionTimeMs: ['reactiontimems','reactiontime','reaction','tiemporeaccion','responsetimems','latencyms'],
  targetX: ['targetx','objetivox','posx','positionx'], targetY: ['targety','objetivoy','posy','positiony'],
  clickX: ['clickx','mousex','clicx','impactx'], clickY: ['clicky','mousey','clicy','impacty']
};

function keyName(value) { return String(value).toLowerCase().replace(/[^a-z0-9áéíóúñ]/g, ''); }
function pick(row, name) {
  const lookup = Object.fromEntries(Object.entries(row).map(([k,v]) => [keyName(k), v]));
  for (const alias of aliases[name]) if (lookup[alias] !== undefined && lookup[alias] !== '') return lookup[alias];
  return undefined;
}
function num(v) {
  if (v === undefined || v === null || v === '') return null;
  const n = Number(String(v).replace(',', '.'));
  return Number.isFinite(n) ? n : null;
}
function typeOf(v) {
  const s = keyName(v || 'click');
  if (['hit','acierto','success','targethit'].includes(s)) return 'hit';
  if (['miss','error','fallo','targetexpired','expired'].includes(s)) return 'miss';
  return 'click';
}
function normalizeRows(rows, fallback) {
  return rows.map((row, i) => ({
    sessionId: String(pick(row,'sessionId') ?? fallback ?? 'Partida 1'),
    timestamp: pick(row,'timestamp') ?? new Date(Date.now() + i * 1000).toISOString(),
    eventType: typeOf(pick(row,'eventType')),
    reactionTimeMs: num(pick(row,'reactionTimeMs')),
    targetX: num(pick(row,'targetX')), targetY: num(pick(row,'targetY')),
    clickX: num(pick(row,'clickX')), clickY: num(pick(row,'clickY'))
  }));
}
function parseCSV(text, fallback) {
  const lines = text.replace(/^\uFEFF/, '').split(/\r?\n/).filter(x => x.trim());
  if (lines.length < 2) throw new Error('El CSV no contiene filas de datos.');
  const split = (line, delimiter) => {
    const out=[]; let cur=''; let quote=false;
    for (let i=0;i<line.length;i++) { const c=line[i]; if(c==='"' && line[i+1]==='"'){cur+='"';i++;} else if(c==='"') quote=!quote; else if(c===delimiter && !quote){out.push(cur.trim());cur='';} else cur+=c; }
    out.push(cur.trim()); return out;
  };
  const delimiter = (lines[0].match(/;/g)||[]).length > (lines[0].match(/,/g)||[]).length ? ';' : ',';
  const headers=split(lines[0],delimiter);
  return normalizeRows(lines.slice(1).map(line => Object.fromEntries(split(line,delimiter).map((v,i)=>[headers[i],v]))), fallback);
}
function parseJSON(text, fallback) {
  const data = JSON.parse(text);
  if (Array.isArray(data)) return normalizeRows(data, fallback);
  if (Array.isArray(data.events)) return normalizeRows(data.events.map(e => ({...e, sessionId: e.sessionId ?? data.sessionId, timestamp: e.timestamp ?? data.timestamp})), fallback);
  if (Array.isArray(data.sessions)) return data.sessions.flatMap(s => normalizeRows(s.events || [], s.sessionId || s.id || fallback));
  throw new Error('El JSON debe contener un array de eventos, events o sessions.');
}
function groupSessions(events) {
  const groups = new Map();
  for (const event of events) {
    if (!groups.has(event.sessionId)) groups.set(event.sessionId, []);
    groups.get(event.sessionId).push(event);
  }
  return [...groups.entries()].map(([id, ev]) => ({ id, events: ev, startedAt: new Date(ev.map(x => new Date(x.timestamp).getTime()).filter(Number.isFinite).sort()[0] || Date.now()) })).sort((a,b)=>a.startedAt-b.startedAt);
}
function addEvents(events) {
  const existing = state.sessions.flatMap(s=>s.events);
  state.sessions = groupSessions([...existing,...events]);
  state.selected = new Set(state.sessions.map(s=>s.id));
  persist(); refreshControls(); render();
}
function persist() { try { localStorage.setItem('aimlab-events', JSON.stringify(state.sessions.flatMap(s=>s.events))); } catch {} }
function restore() { try { const saved=JSON.parse(localStorage.getItem('aimlab-events')||'[]'); if(saved.length){state.sessions=groupSessions(saved);state.selected=new Set(state.sessions.map(s=>s.id));} } catch {} }
function summary(sessionOrEvents) {
  const ev = Array.isArray(sessionOrEvents) ? sessionOrEvents : sessionOrEvents?.events || [];
  const hits=ev.filter(x=>x.eventType==='hit').length, misses=ev.filter(x=>x.eventType==='miss').length, clicks=ev.filter(x=>x.eventType==='click').length;
  const total=hits+misses+clicks; const reactions=ev.map(x=>x.reactionTimeMs).filter(Number.isFinite);
  return { hits, misses, clicks, total, accuracy: total ? hits/total*100 : 0, avgReaction: reactions.length ? reactions.reduce((a,b)=>a+b,0)/reactions.length : 0, bestReaction: reactions.length ? Math.min(...reactions) : 0 };
}
function selectedSessions() { return state.sessions.filter(s=>state.selected.has(s.id)); }
function selectedEvents() { return selectedSessions().flatMap(s=>s.events).filter(e=>state.eventType==='all'||e.eventType===state.eventType); }
function latestSession() { const list=selectedSessions(); return list[list.length-1]; }
function comparisonSession() { const latest=latestSession(); if(!latest)return null; if(state.compareId)return state.sessions.find(s=>s.id===state.compareId); const i=state.sessions.findIndex(s=>s.id===latest.id); return state.sessions[i-1]||null; }
function fmt(n,d=0){return Number.isFinite(n)?n.toLocaleString('es-ES',{maximumFractionDigits:d,minimumFractionDigits:d}):'—';}
function delta(current, previous, inverse=false) {
  if (!previous || !Number.isFinite(current) || !Number.isFinite(previous) || previous===0) return {text:'Sin referencia',class:''};
  const pct=(current-previous)/previous*100; const good=inverse?pct<0:pct>0;
  return {text:`${pct>=0?'+':''}${fmt(pct,1)}% vs. comparación`,class:good?'good':pct===0?'':'bad'};
}
function refreshControls() {
  const multi=$('#sessionFilter'), compare=$('#compareSelect');
  const allSelected=state.selected.size===state.sessions.length;
  multi.innerHTML=`<option value="all" ${allSelected?'selected':''}>Todas las partidas</option>`+state.sessions.map(s=>`<option value="${escapeHtml(s.id)}" ${!allSelected&&state.selected.has(s.id)?'selected':''}>${escapeHtml(s.id)}</option>`).join('');
  compare.innerHTML='<option value="">Partida anterior</option>'+state.sessions.map(s=>`<option value="${escapeHtml(s.id)}">${escapeHtml(s.id)}</option>`).join('');
  compare.value=state.compareId;
}
function render() {
  const sessions=selectedSessions(), latest=latestSession(), prev=comparisonSession(), current=summary(latest), before=summary(prev), aggregate=summary(selectedEvents());
  $('#activeRange').textContent=sessions.length ? `${sessions.length} partida${sessions.length===1?'':'s'} · ${selectedEvents().length} eventos` : 'Sin datos';
  const cards=[
    ['Precisión',`${fmt(current.accuracy,1)}%`,delta(current.accuracy,before.accuracy)],
    ['Reacción media',current.avgReaction?`${fmt(current.avgReaction)} ms`:'—',delta(current.avgReaction,before.avgReaction,true)],
    ['Mejor reacción',current.bestReaction?`${fmt(current.bestReaction)} ms`:'—',delta(current.bestReaction,before.bestReaction,true)],
    ['Aciertos',fmt(current.hits),delta(current.hits,before.hits)],
    ['Errores y vacíos',fmt(current.misses+current.clicks),delta(current.misses+current.clicks,before.misses+before.clicks,true)]
  ];
  $('#metricGrid').innerHTML=cards.map(([l,v,d])=>`<article class="metric"><span class="label">${l}</span><strong class="value">${v}</strong><span class="delta ${d.class}">${d.text}</span></article>`).join('');
  renderEvolution(sessions); renderDonut(aggregate); renderPositions(selectedEvents()); renderTable(selectedEvents());
  $('#status').textContent=state.sessions.length?`Datos listos. La tarjeta superior compara “${latest?.id}” con “${prev?.id||'sin referencia'}”.`:'Carga archivos o prueba el ejemplo para activar el panel.';
}
function renderEvolution(sessions) {
  const el=$('#evolutionChart'); if(!sessions.length){el.innerHTML='<div class="empty">Carga dos o más partidas para ver la evolución.</div>';return;}
  const data=sessions.map(s=>({id:s.id,...summary(s)})), W=760,H=260,p={l:42,r:42,t:20,b:34}, iw=W-p.l-p.r,ih=H-p.t-p.b;
  const maxReaction=Math.max(...data.map(x=>x.avgReaction),1)*1.15;
  const x=i=>p.l+(data.length===1?iw/2:i*iw/(data.length-1)); const ya=v=>p.t+ih-(v/100)*ih; const yr=v=>p.t+ih-(v/maxReaction)*ih;
  const path=(key,y)=>data.map((d,i)=>`${i?'L':'M'}${x(i).toFixed(1)},${y(d[key]).toFixed(1)}`).join(' ');
  const labels=data.map((d,i)=>`<text class="axis-label" x="${x(i)}" y="${H-8}" text-anchor="middle">${escapeHtml(d.id.slice(0,12))}</text>`).join('');
  const grids=[0,25,50,75,100].map(v=>`<line class="grid-line" x1="${p.l}" x2="${W-p.r}" y1="${ya(v)}" y2="${ya(v)}"/><text class="axis-label" x="2" y="${ya(v)+4}">${v}%</text>`).join('');
  const points=data.map((d,i)=>`<circle class="point" fill="var(--cyan)" cx="${x(i)}" cy="${ya(d.accuracy)}" r="5"><title>${d.id}: ${fmt(d.accuracy,1)}%</title></circle><circle class="point" fill="var(--violet)" cx="${x(i)}" cy="${yr(d.avgReaction)}" r="4"><title>${d.id}: ${fmt(d.avgReaction)} ms</title></circle>`).join('');
  const area=`${path('accuracy',ya)} L${x(data.length-1)},${p.t+ih} L${x(0)},${p.t+ih} Z`;
  el.innerHTML=`<svg viewBox="0 0 ${W} ${H}" role="img"><defs><linearGradient id="areaGradient" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#32e5c4" stop-opacity=".22"/><stop offset="1" stop-color="#32e5c4" stop-opacity="0"/></linearGradient></defs>${grids}<path class="area-accuracy" d="${area}"/><path class="line-accuracy" d="${path('accuracy',ya)}"/><path class="line-reaction" d="${path('avgReaction',yr)}"/>${points}${labels}<text class="axis-label" x="${W-38}" y="${p.t+4}" text-anchor="end">${fmt(maxReaction)} ms</text></svg>`;
}
function renderDonut(s) {
  const el=$('#outcomeChart'); if(!s.total){el.innerHTML='<div class="empty">Aún no hay eventos para distribuir.</div>';return;}
  const hp=s.hits/s.total*100, mp=s.misses/s.total*100;
  el.innerHTML=`<div class="donut" style="background:conic-gradient(var(--cyan) 0 ${hp}%,var(--red) ${hp}% ${hp+mp}%,var(--amber) ${hp+mp}% 100%)"><div class="donut-center"><strong>${fmt(s.total)}</strong><span>CLICS</span></div></div><div class="donut-legend"><div><span>Aciertos</span><b>${s.hits}</b></div><div><span>Errores</span><b>${s.misses}</b></div><div><span>Vacíos</span><b>${s.clicks}</b></div></div>`;
}
function normalizeCoord(v, values) {
  if(!Number.isFinite(v))return 50; if(v>=0&&v<=1)return v*100; if(v>=-1&&v<=1)return (v+1)*50;
  const finite=values.filter(Number.isFinite), min=Math.min(...finite), max=Math.max(...finite); return max===min?50:(v-min)/(max-min)*100;
}
function renderPositions(events) {
  const el=$('#positionMap'), positioned=events.filter(e=>Number.isFinite(e.clickX??e.targetX)&&Number.isFinite(e.clickY??e.targetY));
  if(!positioned.length){el.innerHTML='<div class="empty">Incluye coordenadas X/Y para descubrir patrones espaciales.</div>';return;}
  const xs=positioned.map(e=>e.clickX??e.targetX),ys=positioned.map(e=>e.clickY??e.targetY);
  el.innerHTML=positioned.slice(-180).map(e=>{const x=normalizeCoord(e.clickX??e.targetX,xs),y=100-normalizeCoord(e.clickY??e.targetY,ys);return `<i class="impact ${e.eventType}" style="left:${x}%;top:${y}%"><title>${escapeHtml(e.sessionId)} · ${e.eventType} · ${fmt(e.reactionTimeMs)} ms</title></i>`;}).join('');
}
function renderTable(events) {
  const rows=[...events].sort((a,b)=>new Date(b.timestamp)-new Date(a.timestamp)).slice(0,80);
  $('#eventTable').innerHTML=rows.length?rows.map(e=>`<tr><td>${escapeHtml(e.sessionId)}</td><td><span class="event-pill ${e.eventType}">${e.eventType==='hit'?'acierto':e.eventType==='miss'?'error':'vacío'}</span></td><td>${e.reactionTimeMs?fmt(e.reactionTimeMs)+' ms':'—'}</td><td>${Number.isFinite(e.clickX??e.targetX)?`${fmt(e.clickX??e.targetX,2)}, ${fmt(e.clickY??e.targetY,2)}`:'—'}</td></tr>`).join(''):'<tr><td colspan="4"><div class="empty">No hay eventos con este filtro.</div></td></tr>';
}
function escapeHtml(v){return String(v).replace(/[&<>'"]/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#039;','"':'&quot;'}[c]));}
async function readFiles(files) {
  let imported=[]; const errors=[];
  for(const file of files){try{const text=await file.text();const fallback=file.name.replace(/\.[^.]+$/,'');imported.push(...(file.name.toLowerCase().endsWith('.json')?parseJSON(text,fallback):parseCSV(text,fallback)));}catch(e){errors.push(`${file.name}: ${e.message}`);}}
  if(imported.length){addEvents(imported);$('#status').textContent=`Importados ${imported.length} eventos de ${files.length-errors.length} archivo(s).`;$('#status').className='status';}
  if(errors.length){$('#status').textContent=`No se pudo leer: ${errors.join(' · ')}`;$('#status').className='status error';}
}
function downloadCSV(){const ev=selectedEvents();if(!ev.length)return;const cols=['sessionId','timestamp','eventType','reactionTimeMs','targetX','targetY','clickX','clickY'];const csv=[cols.join(','),...ev.map(r=>cols.map(c=>JSON.stringify(r[c]??'')).join(','))].join('\n');const a=document.createElement('a');a.href=URL.createObjectURL(new Blob([csv],{type:'text/csv'}));a.download='aim-trainer-vista.csv';a.click();URL.revokeObjectURL(a.href);}
function wire() {
  $('#selectFiles').onclick=()=>$('#fileInput').click(); $('#fileInput').onchange=e=>readFiles([...e.target.files]);
  const dz=$('#dropZone'); ['dragenter','dragover'].forEach(n=>dz.addEventListener(n,e=>{e.preventDefault();dz.classList.add('dragging');})); ['dragleave','drop'].forEach(n=>dz.addEventListener(n,e=>{e.preventDefault();dz.classList.remove('dragging');})); dz.addEventListener('drop',e=>readFiles([...e.dataTransfer.files]));
  $('#loadDemo').onclick=async()=>{try{const r=await fetch('./aim-trainer-example.json');addEvents(parseJSON(await r.text(),'Demo'));}catch{$('#status').textContent='No se pudieron cargar los datos de ejemplo.';}};
  $('#sessionFilter').onchange=e=>{state.selected=e.target.value==='all'?new Set(state.sessions.map(s=>s.id)):new Set([e.target.value]);render();};
  $('#compareSelect').onchange=e=>{state.compareId=e.target.value;render();}; $('#eventFilter').onchange=e=>{state.eventType=e.target.value;render();};
  $('#clearData').onclick=()=>{state.sessions=[];state.selected.clear();localStorage.removeItem('aimlab-events');refreshControls();render();};
  $('#exportButton').onclick=downloadCSV; $('#themeButton').onclick=()=>document.documentElement.classList.toggle('light');
}
restore();wire();refreshControls();render();
