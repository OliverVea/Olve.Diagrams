const express = require('express');
const multer = require('multer');
const puppeteer = require('puppeteer');

const app = express();
const upload = multer();
const PORT = 3000;

const renderMermaidSVG = async (code) => {
  const browser = await puppeteer.launch({ headless: 'new', args: ['--no-sandbox'] });
  const page = await browser.newPage();
  await page.setContent(`
    <html>
      <body>
        <div class="mermaid">${code}</div>
        <script type="module">
          import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
          mermaid.initialize({ startOnLoad: true });
        </script>
      </body>
    </html>
  `, { waitUntil: 'networkidle0' });

  const svg = await page.$eval('.mermaid', el => el.innerHTML);
  await browser.close();
  return svg;
};

app.post('/upload', upload.single('file'), async (req, res) => {
  try {
    const code = req.file.buffer.toString('utf-8');
    const svg = await renderMermaidSVG(code);
    res.set('Content-Type', 'image/svg+xml');
    res.send(svg);
  } catch (err) {
    console.error(err);
    res.status(500).send('Error rendering Mermaid diagram');
  }
});

app.listen(PORT, () => {
  console.log(`Server running at http://localhost:${PORT}`);
});
