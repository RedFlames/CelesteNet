//@ts-check
import { rd, rdom, rd$, escape$, RDOMListHelper } from "../../../js/rdom.js";
import mdcrd from "../utils/mdcrd.js";

/**
 * @typedef {import("material-components-web")} mdc
 */
/** @type {import("material-components-web")} */
const mdc = window["mdc"]; // mdc

export class FrontendBasicPanel {
  /**
   * @param {import("../frontend.js").Frontend} frontend
   */
  constructor(frontend) {
    this.frontend = frontend;

    /** @type {string} */
    this.id = null;
    this.header = "Header";
    this.subheader = "";

    /** @type {HTMLElement} */
    this.elBody = null;

    this.progress = 2;

    this.updateDelay = 100;

    /** @type {[string | ((el: HTMLElement) => HTMLElement), () => void][] | [string | ((el: HTMLElement) => HTMLElement)][]} */
    this.list = null;

    /** @type {[string, string, () => void][]} */
    this.actions = [
      ["Refresh", "refresh", () => this.refresh()]
    ];
  }

  async start() {
    this.refresh();
  }

  async refresh() {
    if (this.progress !== 2) {
      this.progress = 2;
      this.render();
    }

    if (!this.updatePromise) {
      this.updatePromise = new Promise((resolve, reject) => {
        this.updatePromiseResolve = (...args) => {
          resolve(...args);
          this.updatePromise = null;
        };
        this.updatePromiseReject = (...args) => {
          reject(...args);
          this.updatePromise = null;
        };
      });
    }

    if (this.updateTimeout)
      clearTimeout(this.updateTimeout);
    this.updateTimeout = setTimeout(() => {
      this.update().then(
        (...args) => this.updatePromiseResolve(...args),
        (...args) => this.updatePromiseReject(...args)
      );
    }, this.updateDelay);

    await this.updatePromise;

    this.progress = 0;
    this.render();
  }

  async update() {
  }

  render(el) {
    return this.el = rd$(el || this.el)`
    <div class="panel" ${rd.toggleClass("panelType", "panel-" + this.id)}=${true}>
      ${el => this.renderHeader(el)}
      ${mdcrd.progress(this.progress)}
      ${el => this.renderBody(el)}
    </div>`;
  }

  renderHeader(el) {
    return this.elHeader = rd$(el || this.elHeader)`
    <h2>
      ${this.header}
      ${el => this.renderSubheader(el)}
      ${el => {
        el = rd$(el)`<div class="actions"></div>`;

        let list = new RDOMListHelper(el);
        for (let i in this.actions) {
          let action = this.actions[i];
          // @ts-ignore
          let actionEl = list.add(i, mdcrd.iconButton(...action));
          this.frontend.dom.setTooltip(actionEl, action[0]);
        }

        return el;
      }}
    </h2>`;
  }

  renderSubheader(el) {
    if (this.subheader.length > 0)
      return this.elSubheader = rd$(el || this.elSubheader)`
      <div class="header-small-info">
      ${this.subheader}
      </div>`;

    return this.elSubheader = null;
  }

  renderBody(el) {
    if (this.list)
      return this.elBody = rd$(el || this.elBody)`
      <div class="panel-list">
        ${mdcrd.list.list(...this.list)}
      </div>`

    return this.elBody = null;
  }

}
