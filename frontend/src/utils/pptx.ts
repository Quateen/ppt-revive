import JSZip from 'jszip';
import { XMLParser, XMLBuilder } from 'fast-xml-parser';

export async function extractSlidesXml(file: File): Promise<Record<string, string>> {
  const buffer = await file.arrayBuffer();
  const zip = await JSZip.loadAsync(buffer);
  const slidesXml: Record<string, string> = {};

  const files = Object.keys(zip.files);
  for (const fileName of files) {
    if (fileName.startsWith('ppt/slides/slide') && fileName.endsWith('.xml')) {
      const xml = await zip.files[fileName].async('string');
      slidesXml[fileName] = xml;
    }
  }

  return slidesXml;
}

export async function replaceTextInSlideXml(
  xmlContent: string,
  replacements: Record<string, string>
): Promise<string> {
  const parser = new XMLParser({ ignoreAttributes: false });
  const builder = new XMLBuilder({ ignoreAttributes: false });

  const slideObj = parser.parse(xmlContent);
  const shapes = slideObj['p:sld']?.['p:cSld']?.['p:spTree']?.['p:sp'] ?? [];

  const shapeArray = Array.isArray(shapes) ? shapes : [shapes];

  for (const shape of shapeArray) {
    const txBody = shape['p:txBody'];
    if (!txBody) continue;

    const paragraphs = txBody['a:p'];
    const paragraphArray = Array.isArray(paragraphs) ? paragraphs : [paragraphs];

    for (const p of paragraphArray) {
      const runs = p['a:r'] ?? [];
      const runArray = Array.isArray(runs) ? runs : [runs];

      for (const run of runArray) {
        const textNode = run['a:t'];
        if (typeof textNode === 'string') {
          const oldText = textNode;
          if (replacements[oldText]) {
            run['a:t'] = replacements[oldText];
          }
        }
      }
    }
  }

  return builder.build(slideObj);
}

export async function buildUpdatedPptx(
  originalZip: JSZip,
  updatedSlides: Record<string, string>
): Promise<Blob> {
  const updatedZip = new JSZip();

  for (const fileName of Object.keys(originalZip.files)) {
    const file = originalZip.files[fileName];
    if (updatedSlides[fileName]) {
      updatedZip.file(fileName, updatedSlides[fileName]);
    } else {
      const fileBuffer = await file.async('uint8array');
      updatedZip.file(fileName, fileBuffer);
    }
  }

  return updatedZip.generateAsync({ type: 'blob' });
}
