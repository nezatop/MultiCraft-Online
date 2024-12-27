import path from 'path';

export const PORT = process.env.PORT || 8080;
export const JSON_FILE_PATH = path.join(process.cwd(), 'players.json');
