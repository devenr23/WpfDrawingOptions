import { Application, Graphics } from 'pixi.js';

(async () => {
    const app = new Application();
    await app.init({
        background: '#000000',
        resizeTo: window,
    });

    document.body.appendChild(app.canvas);

    const g = new Graphics();
    app.stage.addChild(g);

    const NUM_LINES = 5000;
    const w = app.screen.width;
    const h = app.screen.height;
    app.ticker.add(() => {


        g.clear();

        for (let i = 0; i < NUM_LINES; i++) {
            const x1 = Math.random() * w;
            const y1 = Math.random() * h;
            const x2 = Math.random() * w;
            const y2 = Math.random() * h;

            const r = Math.floor(Math.random() * 255);
            const gCol = Math.floor(Math.random() * 255);
            const b = Math.floor(Math.random() * 255);
            const a = Math.random();
            const width = Math.random() * 8 + 1;

            g.moveTo(x1, y1)
                .lineTo(x2, y2)
                .stroke({
                    width,
                    color: (r << 16) + (gCol << 8) + b,
                    alpha: a,
                });
        }
    });
})();