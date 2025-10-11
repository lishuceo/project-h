import * as PIXI from 'pixi.js';

(async function main(){
  const LOGICAL_W = 12;
  const LOGICAL_H = 22;
  const SCALE = 10;
  const PIXEL_W = LOGICAL_W * SCALE;
  const PIXEL_H = LOGICAL_H * SCALE;
  const COLORS = [0xff5151, 0x4da3ff, 0x4ad97c, 0xffd23f];
  const GameState = { IDLE: '空闲', DRAG: '拖动', SETTLING: '结算', ANIM: '动画', GAMEOVER: '结束' };

  const app = new PIXI.Application();
  await app.init({ backgroundAlpha: 0, antialias: false, resolution: 1, width: 600, height: 600 });
  document.getElementById('app').appendChild(app.canvas);

  const boardViewport = new PIXI.Container();
  app.stage.addChild(boardViewport);

  const backdrop = new PIXI.Graphics();
  boardViewport.addChild(backdrop);

  const stableLayer = new PIXI.Graphics();
  const activeLayer = new PIXI.Graphics();
  boardViewport.addChild(stableLayer);
  boardViewport.addChild(activeLayer);

  let stableLayerDirty = true;

  function layout() {
    const W = window.innerWidth;
    const H = window.innerHeight;
    app.renderer.resize(W, H);
    const margin = 12;
    // 竖屏优先，尽量让棋盘高度占满，左右留少量边距，居中显示
    const maxBoardHeight = H - margin * 2 - 160; // 底部预留给候选区
    const scale = Math.max(1, Math.floor(maxBoardHeight / PIXEL_H));
    const drawW = PIXEL_W * scale;
    const drawH = PIXEL_H * scale;
    boardViewport.x = Math.floor((W - drawW) / 2);
    boardViewport.y = margin;
    boardViewport.scale.set(scale);

    backdrop.clear();
    backdrop.roundRect(0, 0, PIXEL_W, PIXEL_H, 2).fill({ color: 0x0f141d, alpha: 1 });
    backdrop.rect(0, 0, PIXEL_W, PIXEL_H).stroke({ color: 0x2a3342, width: 1/scale });
  }
  window.addEventListener('resize', layout);
  layout();

  class Pixel {
    constructor(x, y, color, groupId) {
      this.x = x; this.y = y; this.color = color; this.groupId = groupId;
      this.isStable = false;
      this.updatedThisFrame = false;
    }
  }

  let pixelGrid = [];
  function resetGrid() {
    pixelGrid = new Array(PIXEL_H);
    for (let y = 0; y < PIXEL_H; y++) pixelGrid[y] = new Array(PIXEL_W).fill(null);
  }
  resetGrid();

  class PhysicsManager {
    constructor() { this.active = new Set(); }
    add(pixel) { pixel.isStable = false; this.active.add(pixel); }
    addMany(pixels) { for (const p of pixels) this.add(p); }
    get allStable() { return this.active.size === 0; }
    update() {
      for (const p of this.active) p.updatedThisFrame = false;
      const arr = Array.from(this.active);
      arr.sort((a, b) => b.y - a.y);
      for (const p of arr) {
        if (p.isStable || p.updatedThisFrame) continue;
        if (updatePixelPhysics(p)) p.updatedThisFrame = true; else { p.isStable = true; this.active.delete(p); stableLayerDirty = true; }
      }
    }
  }
  const physics = new PhysicsManager();

  function canMoveTo(x, y) {
    if (x < 0 || x >= PIXEL_W || y < 0 || y >= PIXEL_H) return false;
    return pixelGrid[y][x] === null;
  }
  function movePixelTo(pixel, nx, ny) {
    pixelGrid[pixel.y][pixel.x] = null;
    pixel.x = nx; pixel.y = ny;
    pixelGrid[ny][nx] = pixel;
  }
  function updatePixelPhysics(pixel) {
    const x = pixel.x, y = pixel.y;
    if (y + 1 < PIXEL_H && canMoveTo(x, y + 1)) { movePixelTo(pixel, x, y + 1); return true; }
    const leftFirst = Math.random() > 0.5;
    const dirs = leftFirst ? [-1, 1] : [1, -1];
    for (const d of dirs) {
      const nx = x + d, ny = y + 1;
      if (nx >= 0 && nx < PIXEL_W && ny < PIXEL_H && canMoveTo(nx, ny)) { movePixelTo(pixel, nx, ny); return true; }
    }
    return false;
  }

  function recheckStability() {
    let count = 0;
    for (let y = 0; y < PIXEL_H; y++) {
      for (let x = 0; x < PIXEL_W; x++) {
        const p = pixelGrid[y][x];
        if (!p || !p.isStable) continue;
        if (!hasStableSupportBelow(x, y)) { p.isStable = false; physics.active.add(p); count++; }
      }
    }
    if (count > 0) stableLayerDirty = true;
    return count;
  }
  function isEmptyOrUnstable(x, y) {
    if (x < 0 || x >= PIXEL_W || y < 0 || y >= PIXEL_H) return false;
    const p = pixelGrid[y][x];
    return p === null || !p.isStable;
  }
  function hasStableSupportBelow(x, y) {
    const d = isEmptyOrUnstable(x, y + 1);
    const ld = isEmptyOrUnstable(x - 1, y + 1);
    const rd = isEmptyOrUnstable(x + 1, y + 1);
    return !d && !ld && !rd;
  }

  const SHAPES = {
    I: [{x:0,y:0},{x:1,y:0},{x:2,y:0},{x:3,y:0}],
    O: [{x:0,y:0},{x:1,y:0},{x:0,y:1},{x:1,y:1}],
    T: [{x:0,y:0},{x:1,y:0},{x:2,y:0},{x:1,y:1}],
    L: [{x:0,y:0},{x:0,y:1},{x:1,y:1},{x:2,y:1}],
    J: [{x:2,y:0},{x:0,y:1},{x:1,y:1},{x:2,y:1}],
    S: [{x:1,y:0},{x:2,y:0},{x:0,y:1},{x:1,y:1}],
    Z: [{x:0,y:0},{x:1,y:0},{x:1,y:1},{x:2,y:1}],
  };
  const SHAPE_KEYS = Object.keys(SHAPES);
  function rotateCells(cells) { const maxY = Math.max(...cells.map(c=>c.y)); return cells.map(c=>({ x: maxY - c.y, y: c.x })); }
  class Tetromino { constructor(shapeKey, color){ this.shapeKey=shapeKey; this.color=color; this.cells=SHAPES[shapeKey].map(c=>({...c})); } rotate(){ this.cells = rotateCells(this.cells); } }
  class BagSystem { constructor(){ this.bag=[]; } refill(){ this.bag = SHAPE_KEYS.slice(); shuffle(this.bag); } next(){ if(this.bag.length===0) this.refill(); return this.bag.pop(); } }
  const bag = new BagSystem(); bag.refill();
  function randColor(){ return COLORS[Math.floor(Math.random()*COLORS.length)]; }
  function shuffle(a){ for(let i=a.length-1;i>0;i--){ const j=Math.floor(Math.random()*(i+1)); [a[i],a[j]]=[a[j],a[i]]; } }

  const slots = [null, null, null];
  function refillSlot(i){ const key=bag.next(); const color=randColor(); slots[i]=new Tetromino(key, color); drawSlot(i); }
  function initSlots(){ refillSlot(0); refillSlot(1); refillSlot(2); }
  initSlots();

  function drawSlot(i){
    const el = document.querySelector(`.slot[data-slot="${i}"] canvas`);
    const ctx = el.getContext('2d');
    ctx.clearRect(0,0,el.width,el.height);
    const t = slots[i]; if(!t) return;
    // 用真实颜色渲染预览
    const fill = t.color;
    const cellSize = 20;
    const minX = Math.min(...t.cells.map(c=>c.x));
    const minY = Math.min(...t.cells.map(c=>c.y));
    const w = (Math.max(...t.cells.map(c=>c.x)) - minX + 1) * cellSize;
    const h = (Math.max(...t.cells.map(c=>c.y)) - minY + 1) * cellSize;
    const ox = (el.width - w)/2, oy = (el.height - h)/2;
    for(const c of t.cells){
      const x = ox + (c.x - minX) * cellSize;
      const y = oy + (c.y - minY) * cellSize;
      ctx.fillStyle = '#0000';
      ctx.fillRect(x, y, cellSize-1, cellSize-1);
      ctx.fillStyle = `#${fill.toString(16).padStart(6,'0')}`;
      ctx.fillRect(x+1, y+1, cellSize-3, cellSize-3);
    }
  }

  // 调试：确保任何状态下都可看到当前gameState
  window.__debugGameState = () => gameState;

  let gameState = GameState.IDLE;
  let dragging = null;
  function setStatus(s){ gameState=s; document.getElementById('status').textContent=s; }

  function screenToGrid(screenX, screenY){
    const scale = boardViewport.scale.x;
    const gx = Math.floor((screenX - boardViewport.x) / scale);
    const gy = Math.floor((screenY - boardViewport.y) / scale);
    return { gx: Math.floor(gx / SCALE), gy: Math.floor(gy / SCALE) };
  }

  function canPlace(tetro, gridX, gridY){
    for(const cell of tetro.cells){
      const cx = gridX + cell.x, cy = gridY + cell.y;
      if (cx < 0 || cx >= LOGICAL_W || cy < 0 || cy >= LOGICAL_H) return false;
      for(let py=0; py<SCALE; py++){
        for(let px=0; px<SCALE; px++){
          const pxX = cx * SCALE + px;
          const pxY = cy * SCALE + py;
          if (pixelGrid[pxY][pxX] !== null) return false;
        }
      }
    }
    return true;
  }

  let nextGroupId = 1;
  function placeTetromino(t, gridX, gridY){
    const groupId = nextGroupId++;
    const newPixels = [];
    for(const cell of t.cells){
      const sx = (gridX + cell.x) * SCALE;
      const sy = (gridY + cell.y) * SCALE;
      for(let py=0; py<SCALE; py++){
        for(let px=0; px<SCALE; px++){
          const x = sx + px, y = sy + py;
          const p = new Pixel(x, y, t.color, groupId);
          pixelGrid[y][x] = p;
          newPixels.push(p);
        }
      }
    }
    physics.addMany(newPixels);
    stableLayerDirty = true;
  }

  for (const slotEl of document.querySelectorAll('.slot')){
    const startDrag = (idx)=>{
      if (gameState === GameState.GAMEOVER) return; // 游戏结束才禁止
      const t = slots[idx];
      if (!t) return;
      dragging = { tetromino: new Tetromino(t.shapeKey, t.color), slotIndex: idx, gridX: 0, gridY: 0 };
      setStatus(GameState.DRAG);
    };
    slotEl.addEventListener('mousedown', (e)=>{
      e.preventDefault();
      const idx = Number(slotEl.getAttribute('data-slot'));
      startDrag(idx);
    });
    slotEl.addEventListener('touchstart', (e)=>{
      e.preventDefault();
      const idx = Number(slotEl.getAttribute('data-slot'));
      startDrag(idx);
    }, { passive: false });
  }

  window.addEventListener('keydown', (e)=>{ if (e.key.toLowerCase()==='r' && dragging){ dragging.tetromino.rotate(); }});
  window.addEventListener('mousemove', (e)=>{ if (!dragging) return; const { gx, gy } = screenToGrid(e.clientX, e.clientY); dragging.gridX=gx; dragging.gridY=gy; });
  window.addEventListener('touchmove', (e)=>{ if (!dragging) return; const t=e.touches[0]; const { gx, gy } = screenToGrid(t.clientX, t.clientY); dragging.gridX=gx; dragging.gridY=gy; }, { passive: false });
  const tryPlace = ()=>{
    if (!dragging) return;
    const { tetromino, gridX, gridY, slotIndex } = dragging;
    // 动画播放中允许拖拽预览，但不允许实际放置
    if (gameState === GameState.ANIM) { dragging = null; return; }
    const ok = canPlace(tetromino, gridX, gridY);
    if (ok){ placeTetromino(tetromino, gridX, gridY); refillSlot(slotIndex); setStatus(GameState.SETTLING); }
    dragging = null;
  };
  window.addEventListener('mouseup', tryPlace);
  window.addEventListener('touchend', tryPlace, { passive: false });

  function canPlaceAnywhere(tetro){
    for (let y=0; y<LOGICAL_H; y++){
      for (let x=0; x<LOGICAL_W; x++){
        if (canPlace(tetro, x, y)) return true;
      }
    }
    return false;
  }
  function checkGameOver(){
    const any = slots.some(t => t && canPlaceAnywhere(t));
    if (!any) setStatus(GameState.GAMEOVER);
  }

  document.getElementById('resetBtn').addEventListener('click', ()=>{ resetGrid(); physics.active.clear(); setScore(0); stableLayerDirty = true; setStatus(GameState.IDLE); checkGameOver(); });

  function findConnectedClusters(){
    const visited = new Uint8Array(PIXEL_W * PIXEL_H);
    const clusters = [];
    const qx = new Int16Array(PIXEL_W * PIXEL_H);
    const qy = new Int16Array(PIXEL_W * PIXEL_H);
    for (let y=0; y<PIXEL_H; y++){
      for (let x=0; x<PIXEL_W; x++){
        const p = pixelGrid[y][x];
        if (!p) continue;
        const idx = y * PIXEL_W + x;
        if (visited[idx]) continue;
        const color = p.color; let head=0, tail=0; let touchesLeft=false, touchesRight=false; const pixels=[];
        visited[idx]=1; qx[tail]=x; qy[tail]=y; tail++;
        while (head < tail){
          const cx=qx[head], cy=qy[head]; head++;
          const cp = pixelGrid[cy][cx]; if (!cp || cp.color !== color) continue;
          pixels.push(cp);
          if (cx===0) touchesLeft=true; if (cx===PIXEL_W-1) touchesRight=true;
          const nx = [cx-1, cx+1, cx, cx]; const ny = [cy, cy, cy-1, cy+1];
          for (let i=0;i<4;i++){
            const ax = nx[i], ay = ny[i]; if (ax<0||ax>=PIXEL_W||ay<0||ay>=PIXEL_H) continue;
            const aidx = ay*PIXEL_W + ax; if (visited[aidx]) continue;
            const ap = pixelGrid[ay][ax]; if (ap && ap.color===color){ visited[aidx]=1; qx[tail]=ax; qy[tail]=ay; tail++; }
          }
        }
        clusters.push({ pixels, touchesLeft, touchesRight, color });
      }
    }
    return clusters;
  }

  function eliminatePixels(pixels){ for(const p of pixels){ if (pixelGrid[p.y][p.x]===p) pixelGrid[p.y][p.x]=null; } stableLayerDirty = true; }

  let score = 0; function setScore(s){ score=s; document.getElementById('score').textContent=String(score); } function addScore(n){ setScore(score+n); }

  async function handleEliminationChain(){
    let chain=0;
    while (true){
      const clusters = findConnectedClusters();
      const targets = clusters.filter(c=>c.touchesLeft && c.touchesRight);
      if (targets.length===0) break; chain++;
      let removed=0; for (const c of targets){ removed+=c.pixels.length; eliminatePixels(c.pixels); }
      addScore(Math.floor(removed/5)*chain);
      do { recheckStability(); await waitUntilAllStable(); } while (!physics.allStable);
    }
  }

  function waitUntilAllStable(){ return new Promise(resolve=>{ const check=()=>{ if (physics.allStable) resolve(); else requestAnimationFrame(check); }; check(); }); }

  app.ticker.add(()=>{
    // 始终允许物理更新（动画阶段加速），并允许拖拽预览，但仅在IDLE/SETTLING完成后才触发消除
    const speed = (gameState === GameState.ANIM) ? 3 : 1; // 动画期间加速
    for (let i=0;i<speed;i++) physics.update();

    if (physics.allStable){
      if (recheckStability()===0){
        if (gameState !== GameState.ANIM && gameState !== GameState.DRAG){
          setStatus(GameState.ANIM);
          handleEliminationChain().then(()=>{ setStatus(GameState.IDLE); checkGameOver(); });
        }
      }
    }
    render();
  });

  function render(){
    if (stableLayerDirty){
      stableLayer.clear();
      for (let y=0;y<PIXEL_H;y++) for (let x=0;x<PIXEL_W;x++){ const p=pixelGrid[y][x]; if (p&&p.isStable){ stableLayer.rect(x,y,1,1).fill(p.color); } }
      stableLayerDirty=false;
    }
    activeLayer.clear();
    for (let y=0;y<PIXEL_H;y++) for (let x=0;x<PIXEL_W;x++){ const p=pixelGrid[y][x]; if (p && !p.isStable){ activeLayer.rect(x,y,1,1).fill(p.color); } }
    if (dragging){
    const { tetromino, gridX, gridY } = dragging;
    const ok = canPlace(tetromino, gridX, gridY);
    const stroke = ok ? 0xffffff : 0xff4d4d;
    const fillColor = tetromino.color; // 拖动时使用原始颜色
    for(const cell of tetromino.cells){
      const sx=(gridX+cell.x)*SCALE; const sy=(gridY+cell.y)*SCALE;
      // 用原色半透明填充 + 细描边，既保留颜色又有可放置提示
      activeLayer.rect(sx, sy, SCALE, SCALE).fill({ color: fillColor, alpha: 0.5 }).stroke({ color: stroke, width: 0.5 });
      // 内部网格线（淡）以保留格子分割感
      for(let i=1;i<SCALE;i+=2){
        activeLayer.rect(sx+i, sy, 1, SCALE).fill({ color: 0xffffff, alpha: 0.10 });
        activeLayer.rect(sx, sy+i, SCALE, 1).fill({ color: 0xffffff, alpha: 0.10 });
      }
    }
  }
  }

  setStatus(GameState.IDLE);
  checkGameOver();
})();
