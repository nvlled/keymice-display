import { $ } from "bun";
import { readdir } from "node:fs/promises";

const GITHUB_TOKEN = Bun.env.GITHUB_TOKEN ?? "";
const RELEASE_VERSION = Bun.env.RELEASE_VERSION ?? "";
const project = {
	owner: "nvlled",
	repo: "keymice-display"
}

if (!GITHUB_TOKEN) {
	console.error("env variable GITHUB_TOKEN must be set");
	process.exit(-1);
}
if (!RELEASE_VERSION) {
	console.error("env variable RELEASE_VERSION must be set, e.g. v1.2.3");
	process.exit(-1);
}

createRelease();

async function createRelease() {
	const { owner, repo } = project

	const resp = await $`curl -L -X POST -H "Accept: application/vnd.github+json" -H "Authorization: Bearer ${GITHUB_TOKEN}" -H "X-GitHub-Api-Version: 2022-11-28" https://api.github.com/repos/${owner}/${repo}/releases -d '{"tag_name":"${RELEASE_VERSION}", "name":"${RELEASE_VERSION}", "draft":true,"prerelease":false,"generate_release_notes":false}' `;

	const id: string = resp.json().id;
	if (!id) {
		console.error("failed to create release");
		process.exit(-1);
	}

	const filenames = (await readdir("../build/")).filter(f => f.endsWith(".zip"));
	console.log({ id, filenames })
	for (const filename of filenames) {
		console.log("uploading", filename)
		await $`curl -L -X POST -H "Accept: application/vnd.github+json" -H "Authorization: Bearer ${GITHUB_TOKEN}" -H "X-GitHub-Api-Version: 2022-11-28" -H "Content-Type: application/octet-stream" "https://uploads.github.com/repos/${owner}/${repo}/releases/${id}/assets?name=${filename}" --data-binary "@../build/${filename}"`
	}
}

